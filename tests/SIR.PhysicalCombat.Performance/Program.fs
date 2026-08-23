module SIR.PhysicalCombat.Performance.Program

open System
open System.Diagnostics
open System.IO
open System.Reflection
open System.Runtime.InteropServices
open System.Security.Cryptography
open System.Text.Json
open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

[<CLIMutable>]
type LimitsReceipt =
    { TraceCells: int
      AreaCells: int
      Recipients: int
      Facts: int
      CanonicalEvidenceBytes: int }

[<CLIMutable>]
type ObservationReceipt =
    { Workload: string
      Units: int
      AttacksRequested: int
      AttacksResolved: int
      AttacksRejected: int
      WarmupIterations: int
      MeasurementIterations: int
      SamplesMilliseconds: int64 array
      P95Milliseconds: int64
      MaximumObserved: LimitsReceipt }

[<CLIMutable>]
type IdentityReceipt =
    { CandidateCommit: string
      SourceTreeState: string
      WorkloadDefinitionSha256: string
      PerformanceAssemblySha256: string
      SimulationAssemblySha256: string
      RulesEngine: string
      RulesCompatibilityProfile: string
      RulesPackageVersion: string
      RulesSourceCommit: string
      RulesImplementationSha256: string
      RulesSemanticSha256: string
      RulesManifestSha256: string }

[<CLIMutable>]
type HostReceipt =
    { OperatingSystem: string
      Architecture: string
      Framework: string
      RuntimeVersion: string
      ProcessorCount: int
      ProcessorModel: string
      MachineName: string }

[<CLIMutable>]
type PerformanceReceipt =
    { Schema: string
      Outcome: string
      ProductionRoute: string
      BuildConfiguration: string
      GeneratedAtUtc: string
      Identity: IdentityReceipt
      Host: HostReceipt
      Limits: LimitsReceipt
      Representative: ObservationReceipt
      Stress: ObservationReceipt
      Mutation: string }

let private required condition message =
    if not condition then failwith message

let private health value =
    BoundedInt32.create 0 100 value |> Result.defaultWith (fun error -> failwithf "Invalid health: %A" error)

let private cell col row: Cell = { Col = col; Row = row }

let private unitState index columns =
    let id = Simulation.unitId (int32 (index + 1))
    id,
    { Id = id
      Side = if index % 2 = 0 then Side.Red else Side.Blue
      Cell = cell (int32 (index % columns)) (int32 (index / columns))
      Health = health 100
      Armor = { FrontRating = 50; RearRating = 20; Integrity = 100 }
      Wounds = []
      Incapacitated = false
      Suppression = 0
      BodyFacing = if index % 2 = 0 then Direction8.East else Direction8.West
      AttentionDirection = Direction8.North
      WeaponPosture = WeaponPosture.Mobile }

let private state unitCount columns rows covers =
    { Tick = 0
      Board =
        { Minimum = cell 0 0
          Maximum = cell (int32 (columns - 1)) (int32 (rows - 1))
          Edges = []
          Covers = covers |> List.map (fun cover -> cover.CoverId, cover) |> Map.ofList }
      Units = [ 0 .. unitCount - 1 ] |> List.map (fun index -> unitState index columns) |> Map.ofList
      Observations = Set.empty
      Awareness = Map.empty
      Engagements = Map.empty
      AwarenessCursor = 0 }

let private representativeWorkload () =
    let profiles =
        [ WeaponProfile.Rifle
          WeaponProfile.SupportWeapon
          WeaponProfile.AntiArmor
          WeaponProfile.LobbedArea ]

    let covers =
        [ { CoverId = "representative-soft-cover"; Cell = cell 2 2; Integrity = 100; ProjectileBlocking = false; Material = "soft"; PenetrationResistance = 0; ProtectedDirections = [] }
          { CoverId = "representative-hard-cover"; Cell = cell 3 3; Integrity = 100; ProjectileBlocking = true; Material = "hard"; PenetrationResistance = 100; ProtectedDirections = [] } ]

    let inputs =
        [ for index in 0 .. 11 ->
            let aimIndex = 5 + index
            KernelInput.PhysicalAttack(
                Simulation.unitId 1,
                cell (int32 (aimIndex % 5)) (int32 (aimIndex / 5)),
                profiles[index % profiles.Length]
            ) ]

    state 25 5 5 covers, inputs

let private stressWorkload () =
    let stressUnit index =
        let id = Simulation.unitId (int32 (index + 1))
        let lattice = index
        id,
        { Id = id
          Side = if index % 2 = 0 then Side.Red else Side.Blue
          Cell = cell (int32 ((lattice % 10) * 2)) (int32 ((lattice / 10) * 2))
          Health = health 100
          Armor = { FrontRating = 50; RearRating = 20; Integrity = 100 }
          Wounds = []
          Incapacitated = false
          Suppression = 0
          BodyFacing = if index % 2 = 0 then Direction8.East else Direction8.West
          AttentionDirection = Direction8.North
          WeaponPosture = WeaponPosture.Mobile }

    let units = [ 0 .. 99 ] |> List.map stressUnit |> Map.ofList

    let initial =
        { Tick = 0
          Board = { Minimum = cell 0 0; Maximum = cell 20 20; Edges = []; Covers = Map.empty }
          Units = units
          Observations = Set.empty
          Awareness = Map.empty
          Engagements = Map.empty
          AwarenessCursor = 0 }

    let inputs =
        [ for index in 1 .. 50 ->
            KernelInput.PhysicalAttack(
                Simulation.unitId 1,
                units[Simulation.unitId (int32 (index + 1))].Cell,
                WeaponProfile.SupportWeapon
            ) ]

    initial, inputs

let private areaCellCount (board: Board) aim profile =
    let radius = (Combat.parameters profile).AreaRadius

    [ for row in aim.Row - radius .. aim.Row + radius do
        for col in aim.Col - radius .. aim.Col + radius do
            let candidate = cell col row
            let distance = max (abs (aim.Col - col)) (abs (aim.Row - row))

            if
                radius > 0
                && distance <= radius
                && candidate.Col >= board.Minimum.Col
                && candidate.Col <= board.Maximum.Col
                && candidate.Row >= board.Minimum.Row
                && candidate.Row <= board.Maximum.Row
            then
                yield candidate ]
    |> List.length

let private factsObservation board (input: KernelInput) event facts =
    let traceCells =
        facts
        |> List.sumBy (function
            | CombatFact.TraceEvaluated(cells, _, _) -> cells.Length
            | _ -> 0)

    let recipients =
        facts
        |> List.choose (function
            | CombatFact.Contact(entityId, _, _) -> Some entityId
            | _ -> None)
        |> Set.ofList
        |> Set.count

    let aim, profile =
        match input with
        | KernelInput.PhysicalAttack(_, aim, profile) -> aim, profile
        | _ -> failwith "Performance workload contains a non-physical input."

    let canonicalEvidenceBytes = Simulation.eventsBytes [ event ] |> Array.length

    { TraceCells = traceCells
      AreaCells = areaCellCount board aim profile
      Recipients = recipients
      Facts = facts.Length
      CanonicalEvidenceBytes = canonicalEvidenceBytes }

let private maximum left right =
    { TraceCells = max left.TraceCells right.TraceCells
      AreaCells = max left.AreaCells right.AreaCells
      Recipients = max left.Recipients right.Recipients
      Facts = max left.Facts right.Facts
      CanonicalEvidenceBytes = max left.CanonicalEvidenceBytes right.CanonicalEvidenceBytes }

let private zeroLimits =
    { TraceCells = 0
      AreaCells = 0
      Recipients = 0
      Facts = 0
      CanonicalEvidenceBytes = 0 }

let private execute workloadName warmups iterations (initialState: SimulationState, inputs: KernelInput list) =
    let run () = Simulation.runPhysicalTickWithRules Simulation.defaultRules initialState inputs

    for _ in 1 .. warmups do
        run () |> ignore

    let mutable last = Unchecked.defaultof<TickResult>

    let samples =
        [| for _ in 1 .. iterations do
            let watch = Stopwatch.StartNew()
            last <- run ()
            watch.Stop()
            yield watch.ElapsedMilliseconds |]

    let resolved =
        last.Events
        |> List.choose (function
            | SimulationEvent.PhysicalAttackResolved(_, _, facts, _) as event -> Some(event, facts)
            | _ -> None)

    let rejected =
        last.Events
        |> List.filter (function SimulationEvent.PhysicalAttackRejected _ -> true | _ -> false)

    let physicalInputs =
        inputs
        |> List.distinct
        |> List.sortBy (function
            | KernelInput.PhysicalAttack(attacker, aim, profile) ->
                Simulation.unitIdValue attacker,
                aim.Col,
                aim.Row,
                (match profile with
                 | WeaponProfile.Rifle -> 0
                 | WeaponProfile.SupportWeapon -> 1
                 | WeaponProfile.AntiArmor -> 2
                 | WeaponProfile.LobbedArea -> 3)
            | _ -> failwith "Performance workload contains a non-physical input.")

    required (resolved.Length + rejected.Length = physicalInputs.Length) "The authoritative tick did not account for every physical attack."
    required (List.isEmpty rejected) "The authoritative tick rejected a performance-workload attack."

    let observed =
        (zeroLimits, List.zip physicalInputs resolved)
        ||> List.fold (fun current (input, (event, facts)) ->
            maximum current (factsObservation initialState.Board input event facts))

    let sortedSamples = samples |> Array.sort
    let p95Index = int (Math.Ceiling(float sortedSamples.Length * 0.95)) - 1

    { Workload = workloadName
      Units = initialState.Units.Count
      AttacksRequested = physicalInputs.Length
      AttacksResolved = resolved.Length
      AttacksRejected = rejected.Length
      WarmupIterations = warmups
      MeasurementIterations = iterations
      SamplesMilliseconds = samples
      P95Milliseconds = sortedSamples[max 0 p95Index]
      MaximumObserved = observed }

let private sha256File path =
    use stream = File.OpenRead path
    SHA256.HashData stream |> Convert.ToHexString |> fun value -> value.ToLowerInvariant()

let private commandOutput (executable: string) (arguments: string) =
    let startInfo: ProcessStartInfo = ProcessStartInfo(executable, arguments)
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo.UseShellExecute <- false
    use child = Process.Start startInfo |> Option.ofObj |> Option.defaultWith (fun () -> failwith $"Identity command could not start: {executable}")
    let output = child.StandardOutput.ReadToEnd().Trim()
    let error = child.StandardError.ReadToEnd().Trim()
    child.WaitForExit()
    required (child.ExitCode = 0) $"Identity command failed: {executable} {arguments}: {error}"
    output

let private sha256Text (value: string) =
    value
    |> Text.Encoding.UTF8.GetBytes
    |> SHA256.HashData
    |> Convert.ToHexString
    |> _.ToLowerInvariant()

let private hex (bytes: byte array) =
    Convert.ToHexString(bytes).ToLowerInvariant()

let private processorModel () =
    let cpuInfo = "/proc/cpuinfo"

    if File.Exists cpuInfo then
        File.ReadLines cpuInfo
        |> Seq.tryPick (fun line ->
            if line.StartsWith("model name", StringComparison.Ordinal) then
                line.Split(':', 2) |> Array.tryItem 1 |> Option.map _.Trim()
            else
                None)
        |> Option.defaultValue "unavailable"
    else
        "unavailable"

let private argument name args =
    args
    |> Array.tryFindIndex ((=) name)
    |> Option.bind (fun index -> args |> Array.tryItem (index + 1))
    |> Option.defaultWith (fun () -> failwithf "Missing required argument %s." name)

let private validSha (value: string) =
    value.Length = 64 && value |> Seq.forall Uri.IsHexDigit

let private verifyAwarenessPerformanceReceipt (receiptPath: string) (expectedCommit: string) =
    required (File.Exists receiptPath) "The awareness performance receipt is unreadable."
    required (expectedCommit.Length = 40 && expectedCommit |> Seq.forall Uri.IsHexDigit) "The awareness candidate commit must be an exact 40-character SHA."
    let exactCommit = expectedCommit.ToLowerInvariant()
    let headCommit = commandOutput "git" "rev-parse HEAD" |> _.ToLowerInvariant()
    required (headCommit = exactCommit) "The checked-out HEAD is not the requested awareness candidate."
    use document = JsonDocument.Parse(File.ReadAllText receiptPath)
    let root = document.RootElement
    let property (name: string) (element: JsonElement) = element.GetProperty name
    let textProperty (name: string) (element: JsonElement) = property name element |> _.GetString() |> Option.ofObj |> Option.defaultValue ""
    let int64Property (name: string) (element: JsonElement) =
        let value = property name element
        required (value.ValueKind = JsonValueKind.Number) $"The awareness receipt {name} observation is not numeric."
        let mutable parsed = 0L
        required (value.TryGetInt64(&parsed)) $"The awareness receipt {name} observation is not a finite integer."
        required (parsed >= 0L) $"The awareness receipt {name} observation is negative."
        parsed
    required (textProperty "schema" root = "sir.awareness-reaction.performance-receipt/1") "The awareness receipt schema is unsupported."
    required (textProperty "outcome" root = "pass") "The awareness receipt did not pass."
    let candidate = property "candidate" root
    required (textProperty "commit" candidate = exactCommit) "The awareness receipt is stale for the requested candidate commit."
    required (String.IsNullOrEmpty(commandOutput "git" "diff --binary HEAD --")) "Exact-candidate awareness acceptance requires a clean working tree."
    required (textProperty "headTree" candidate = commandOutput "git" $"rev-parse {exactCommit}^{{tree}}") "The awareness receipt tree does not belong to the requested candidate."
    required (textProperty "workingTreeState" candidate = "clean") "The awareness receipt was not measured from a clean tree."
    required (textProperty "workingTreeDiffSha256" candidate = sha256Text "") "The awareness receipt contains a working-tree diff."
    required (textProperty "performanceAssemblySha256" candidate = sha256File (Assembly.GetExecutingAssembly().Location)) "The awareness receipt performance assembly is not the verifier assembly."
    required (textProperty "simulationAssemblySha256" candidate = sha256File typeof<SimulationState>.Assembly.Location) "The awareness receipt simulation assembly is not the verifier assembly."
    let workloadPath = "work/182-awareness-reaction-windows/contracts/awareness-reaction-performance-workload-v1.json"
    required (textProperty "workloadDefinitionSha256" root = sha256File workloadPath) "The awareness receipt workload definition is stale."
    use workloadDocument = JsonDocument.Parse(File.ReadAllText workloadPath)
    let workload = workloadDocument.RootElement
    required (JsonElement.DeepEquals(property "workload" root, workload)) "The awareness receipt embedded workload differs from its bound definition."
    let representative = property "representative" workload
    let stress = property "stress" workload
    let caps = property "structuralCapsPerTick" workload
    let observation = property "observation" root
    let p95 = int64Property "p95Milliseconds" observation
    let worst = int64Property "worstMilliseconds" observation
    let units = int64Property "units" observation
    let ticks = int64Property "ticks" observation
    let candidatePairs = int64Property "candidatePairs" observation
    let servicedSlots = int64Property "servicedSlots" observation
    let losEvaluations = int64Property "losEvaluations" observation
    let moves = int64Property "moves" observation
    let engagementObservations = int64Property "engagementObservations" observation
    let reactionCandidates = int64Property "reactionCandidates" observation
    let evidenceBytes = int64Property "evidenceBytes" observation
    let maximumAllocation = int64Property "maximumAllocation" observation
    let measurementTicks = int64Property "measurementTicks" workload
    let warmupTicks = int64Property "warmupTicks" workload
    let expectedUnits = int64Property "units" stress
    let unitsPerSide = int64Property "unitsPerSide" stress
    let hardWorst = int64Property "maximumWorstTickMilliseconds" stress
    required (hardWorst = int64Property "maximumWorstTickMilliseconds" representative) "The awareness workload worst-tick thresholds disagree."
    required (p95 <= worst) "The awareness receipt p95 exceeds its raw worst observation."
    required (worst <= hardWorst) "The awareness receipt raw worst observation exceeds the hard ceiling."
    required (units = expectedUnits && candidatePairs = 2L * unitsPerSide * unitsPerSide) "The awareness receipt workload cardinality is inconsistent."
    let sampledTicks = measurementTicks + warmupTicks + 9L
    required (ticks = sampledTicks + 2L) "The awareness receipt tick count is inconsistent with its workload."
    required (servicedSlots > 0L && servicedSlots <= int64Property "awarenessEpisodes" caps * sampledTicks) "The awareness receipt serviced-slot count exceeds its bound."
    required (losEvaluations > 0L && losEvaluations <= int64Property "losEvaluations" caps * sampledTicks) "The awareness receipt LOS count exceeds its bound."
    required (moves > 0L && moves <= int64Property "events" caps * sampledTicks) "The awareness receipt movement count exceeds its bound."
    required (engagementObservations > 0L && engagementObservations <= int64Property "engagements" caps * sampledTicks) "The awareness receipt engagement count exceeds its bound."
    required (reactionCandidates > 0L && reactionCandidates <= int64Property "reactionFacts" caps * sampledTicks) "The awareness receipt reaction count exceeds its bound."
    required (evidenceBytes <= int64Property "canonicalBytes" caps) "The awareness receipt evidence bytes exceed the canonical cap."
    required (maximumAllocation <= int64Property "maximumAllocationBytes" stress) "The awareness receipt allocation exceeds the hard cap."
    let host = property "host" root
    required (textProperty "operatingSystem" host = RuntimeInformation.OSDescription) "The awareness receipt operating system differs from the verifier host."
    required (textProperty "architecture" host = RuntimeInformation.ProcessArchitecture.ToString()) "The awareness receipt architecture differs from the verifier host."
    required (textProperty "framework" host = RuntimeInformation.FrameworkDescription) "The awareness receipt framework differs from the verifier host."
    required (textProperty "runtimeVersion" host = Environment.Version.ToString()) "The awareness receipt runtime differs from the verifier host."
    required (property "processorCount" host |> _.GetInt32() = Environment.ProcessorCount) "The awareness receipt processor count differs from the verifier host."
    required (textProperty "processorModel" host = processorModel ()) "The awareness receipt processor model differs from the verifier host."
    required (property "gcServer" host |> _.GetBoolean() = System.Runtime.GCSettings.IsServerGC) "The awareness receipt GC mode differs from the verifier host."
    required (textProperty "gcLatencyMode" host = System.Runtime.GCSettings.LatencyMode.ToString()) "The awareness receipt GC latency mode differs from the verifier host."
    printfn "verified-awareness-receipt=%s candidate=%s receipt-sha256=%s" receiptPath exactCommit (sha256File receiptPath)
    0

let private mutatedLimits mutation observation =
    let limits =
        { TraceCells = 256
          AreaCells = 256
          Recipients = 256
          Facts = 4096
          CanonicalEvidenceBytes = 65_536 }

    match mutation with
    | "" -> limits
    | "trace" -> { limits with TraceCells = max 0 (observation.TraceCells - 1) }
    | "area" -> { limits with AreaCells = max 0 (observation.AreaCells - 1) }
    | "recipients" -> { limits with Recipients = max 0 (observation.Recipients - 1) }
    | "facts" -> { limits with Facts = max 0 (observation.Facts - 1) }
    | "evidence-bytes" -> { limits with CanonicalEvidenceBytes = max 0 (observation.CanonicalEvidenceBytes - 1) }
    | value -> failwithf "Unknown cap mutation %s." value

let private within limits observed =
    observed.TraceCells <= limits.TraceCells
    && observed.AreaCells <= limits.AreaCells
    && observed.Recipients <= limits.Recipients
    && observed.Facts <= limits.Facts
    && observed.CanonicalEvidenceBytes <= limits.CanonicalEvidenceBytes

let private awarenessStressState () =
    let make side id position posture =
        let unitId = Simulation.unitId id
        unitId,
        { Id = unitId
          Side = side
          Cell = position
          Health = health 100
          Armor = { FrontRating = 50; RearRating = 20; Integrity = 100 }
          Wounds = []
          Incapacitated = false
          Suppression = 0
          BodyFacing = if side = Side.Red then Direction8.East else Direction8.West
          AttentionDirection = if side = Side.Red then Direction8.East else Direction8.West
          WeaponPosture = posture }
    let reds =
        [ for index in 0 .. 99 ->
            make Side.Red (int32 (index + 1)) (cell (int32 (index % 10)) (int32 (index / 10))) WeaponPosture.Prepared ]
    let blues =
        [ for index in 0 .. 99 ->
            make Side.Blue (int32 (index + 101)) (cell (int32 (20 + (index % 10) * 3)) (int32 ((index / 10) * 3))) WeaponPosture.Mobile ]
    let acquired =
        [ for index in 0 .. 33 do
            let owner = Simulation.unitId (int32 (index + 1))
            let subject = Simulation.unitId (int32 (index + 101))
            yield
                (owner, subject),
                { SubjectId = subject
                  Level = AwarenessLevel.Acquired
                  Acquisition = AwarenessReaction.infantryProfile.IdentificationThreshold
                  LastStimulusTick = Some 0
                  LastStimulus = None
                  LastKnownCell = Some(cell (int32 (20 + (index % 10) * 3)) (int32 ((index / 10) * 3)))
                  RetainUntilTick = Some AwarenessReaction.infantryProfile.LastKnownRetentionTicks
                  Reason = AwarenessReason.IdentificationThresholdReached } ]
        |> Map.ofList
    let guardedEdges =
        [ for index in 0 .. 99 do
            if index % 3 = 2 then
                let baseCell = cell (int32 (20 + (index % 10) * 3)) (int32 ((index / 10) * 3))
                let edge = Edges.edgeBetween baseCell { baseCell with Col = baseCell.Col + 1 } |> Option.defaultWith (fun () -> failwith "invalid stress edge")
                yield { EdgeId = $"stress-edge-{index + 1}"; SpatialRevision = 1; Edge = edge; BlocksMovement = false } ]
    { Tick = 0
      Board = { Minimum = cell 0 0; Maximum = cell 79 79; Edges = guardedEdges; Covers = Map.empty }
      Units = reds @ blues |> Map.ofList
      Observations = Set.empty
      Awareness = acquired
      Engagements = Map.empty
      AwarenessCursor = 0 }

let private runAwarenessPerformance () =
    let mutation = Environment.GetEnvironmentVariable("SIR_AWARENESS_PERF_MUTATE_SUBJECT") |> Option.ofObj |> Option.defaultValue ""
    let workloadPath = Environment.GetEnvironmentVariable("SIR_AWARENESS_WORKLOAD") |> Option.ofObj |> Option.defaultValue "work/182-awareness-reaction-windows/contracts/awareness-reaction-performance-workload-v1.json"
    use workload = JsonDocument.Parse(File.ReadAllText workloadPath)
    let root = workload.RootElement
    let property (name: string) (element: JsonElement) = element.GetProperty name
    let jsonText name element =
        property name element |> _.GetString() |> Option.ofObj |> Option.defaultValue ""
    required (property "schemaVersion" root |> _.GetInt32() = 1) "Awareness workload schema is unreadable."
    required (jsonText "workloadId" root = "sir-awareness-reaction-authoritative-tick-v1") "Awareness workload identity is unreadable."
    required ((jsonText "productionRoute" root).Contains "Simulation.runPhysicalTickWithRules") "Awareness production route is unreadable."
    let warmupTicks = property "warmupTicks" root |> _.GetInt32()
    let measurementTicks = property "measurementTicks" root |> _.GetInt32()
    required (warmupTicks = 10 && measurementTicks = 60) "Awareness warmup/measurement policy changed."
    let representative = property "representative" root
    required ((property "scenarios" representative |> _.GetArrayLength()) = 7) "Awareness representative scenarios changed."
    required ((property "maximumAwarenessP95Milliseconds" representative).ValueKind = JsonValueKind.Null) "Awareness P95 policy changed."
    let workingTarget = property "workingTargetFullTickP95Milliseconds" representative |> _.GetInt32()
    let representativeWorst = property "maximumWorstTickMilliseconds" representative |> _.GetInt32()
    let stress = property "stress" root
    let stressUnits = property "units" stress |> _.GetInt32()
    let unitsPerSide = property "unitsPerSide" stress |> _.GetInt32()
    required (stressUnits = unitsPerSide * 2 && stressUnits = 200) "Awareness stress unit declaration must be the exact balanced 100v100 workload."
    required (property "mapWidth" stress |> _.GetInt32() = 80 && property "mapHeight" stress |> _.GetInt32() = 80 && property "levels" stress |> _.GetInt32() = 2) "Awareness stress map changed."
    required (property "sensorRangeCells" stress |> _.GetInt32() = AwarenessReaction.infantryProfile.MaximumRangeCells) "Awareness sensor range is not bound to production."
    required (property "exposureSamplesPerTarget" stress |> _.GetInt32() = AwarenessReaction.infantryProfile.MaximumExposureSamples) "Awareness exposure samples are not bound to production."
    required (not (String.IsNullOrWhiteSpace(jsonText "motion" stress)) && not (String.IsNullOrWhiteSpace(jsonText "engagements" stress))) "Awareness motion/engagement declarations are unreadable."
    let stressWorstLimit = property "maximumWorstTickMilliseconds" stress |> _.GetInt32()
    let caps = property "structuralCapsPerTick" root
    let expectedCaps = [ "candidatePairs", 20000; "losEvaluations", 5000; "stimuli", 4096; "awarenessEpisodes", 4096; "engagements", 4096; "reactionFacts", 4096; "events", 4096; "coveredAreaCellsPerEngagement", 256; "canonicalBytes", 262144 ]
    expectedCaps |> List.iter (fun (name, expected) -> required (property name caps |> _.GetInt32() = expected) ($"Awareness structural cap changed: {name}."))
    let workloadDigest = sha256File workloadPath
    let candidateCommit = commandOutput "git" "rev-parse HEAD"
    let headTree = commandOutput "git" "rev-parse HEAD^{tree}"
    let workingTreeDiff = commandOutput "git" "diff --binary HEAD --"
    let workingTreeState = if String.IsNullOrEmpty workingTreeDiff then "clean" else "dirty"
    let workingTreeDiffDigest = sha256Text workingTreeDiff
    let prepareInputs =
        [ for index in 0 .. 99 do
            let owner = Simulation.unitId (int32 (index + 1))
            let baseCell = cell (int32 (20 + (index % 10) * 3)) (int32 ((index / 10) * 3))
            yield SetAttention(owner, Direction8.East)
            yield SetWeaponPosture(owner, WeaponPosture.Prepared)
            if mutation <> "no-engagements" then
                match index % 3 with
                | 0 -> yield PrepareUnitReaction(owner, $"stress-unit-{index + 1}", Simulation.unitId (int32 (index + 101)), Direction8.East)
                | 1 -> yield PrepareAreaReaction(owner, $"stress-area-{index + 1}", [ baseCell; { baseCell with Col = baseCell.Col + 1 } ], Direction8.East)
                | _ ->
                    let edge = Edges.edgeBetween baseCell { baseCell with Col = baseCell.Col + 1 } |> Option.defaultWith (fun () -> failwith "invalid stress edge")
                    yield PrepareEdgeReaction(owner, $"stress-edge-{index + 1}", edge, Direction8.East) ]
    let mutable state = awarenessStressState ()
    state <- (Simulation.runTick state prepareInputs).State
    state <- (Simulation.runTick state []).State
    let mutable totalSlots = 0L
    let mutable totalLos = 0L
    let mutable totalMoves = 0
    let mutable totalEngagements = 0L
    let mutable totalReactions = 0L
    let mutable maximumAllocation = 0L
    let mutable evidenceBytes = 0
    let coveredObserverIds = System.Collections.Generic.HashSet<UnitId>()
    let samples = ResizeArray<int64>()
    for sample in 1 .. 79 do
        let moves =
            if mutation = "no-movement" then []
            else
                [ for index in 0 .. 99 do
                    let source = Simulation.unitId (int32 (index + 101))
                    let baseCol = int32 (20 + (index % 10) * 3)
                    let destination = cell (baseCol + (if sample % 2 = 1 then 1 else 0)) (int32 ((index / 10) * 3))
                    yield Move(source, destination) ]
        if mutation = "cursor-reset" then state <- { state with AwarenessCursor = 0 }
        let before = GC.GetAllocatedBytesForCurrentThread()
        let watch = Stopwatch.StartNew()
        let result = Simulation.runTick state moves
        watch.Stop()
        let allocated = GC.GetAllocatedBytesForCurrentThread() - before
        maximumAllocation <- max maximumAllocation allocated
        if mutation = "allocation" then maximumAllocation <- 200_000_000L
        state <- result.State
        state.Awareness |> Map.iter (fun (observerId, _) _ -> coveredObserverIds.Add observerId |> ignore)
        totalSlots <- totalSlots + int64 result.AwarenessCounters.AwarenessEpisodes
        totalLos <- totalLos + int64 result.AwarenessCounters.LosEvaluations
        totalMoves <- totalMoves + (result.Events |> List.sumBy (function UnitMoved _ -> 1 | _ -> 0))
        totalEngagements <- totalEngagements + int64 result.AwarenessCounters.Engagements
        totalReactions <- totalReactions + int64 result.AwarenessCounters.ReactionCandidates
        evidenceBytes <- max evidenceBytes result.EventBytes.Length
        if sample > warmupTicks + 9 then samples.Add watch.ElapsedMilliseconds
    let sorted = samples |> Seq.sort |> Seq.toArray
    let stressP95 = sorted[int (Math.Ceiling(float sorted.Length * 0.95)) - 1]
    let stressWorst = sorted[sorted.Length - 1]
    let coveredObservers = coveredObserverIds.Count
    let passed =
        state.Units.Count = 200
        && state.AwarenessCursor <> 0
        && coveredObservers = 200
        && totalSlots > 20_000L
        && totalLos > 10_000L
        && totalMoves > 0
        && totalEngagements > 0L
        && totalReactions > 0L
        && evidenceBytes <= (property "canonicalBytes" caps |> _.GetInt32())
        && maximumAllocation <= (property "maximumAllocationBytes" stress |> _.GetInt64())
        && stressWorst <= int64 stressWorstLimit
        && samples.Count = measurementTicks
    printfn "route=SIR.Simulation.Simulation.runTick"
    printfn "full-tick-p95-ms=%d target=%d hard-ceiling=%d worst-ms=%d/%d" stressP95 workingTarget representativeWorst stressWorst stressWorstLimit
    printfn "stress-units=%d ticks=%d cursor=%d contacts=%d covered-observers=%d serviced-slots=%d los=%d moves=%d engagement-observations=%d reaction-candidates=%d evidence-bytes=%d/262144 max-allocation=%d/100000000 mutation=%s" state.Units.Count state.Tick state.AwarenessCursor state.Awareness.Count coveredObservers totalSlots totalLos totalMoves totalEngagements totalReactions evidenceBytes maximumAllocation mutation
    let receiptPath =
        Environment.GetEnvironmentVariable("SIR_AWARENESS_PERF_RECEIPT")
        |> Option.ofObj
        |> Option.filter (String.IsNullOrWhiteSpace >> not)
        |> Option.defaultWith (fun () -> failwith "SIR_AWARENESS_PERF_RECEIPT must name an external or temporary receipt path.")
    let receipt =
        {| schema = "sir.awareness-reaction.performance-receipt/1"
           outcome = if passed then "pass" else "fail"
           workloadDefinitionSha256 = workloadDigest
           workload = JsonDocument.Parse(File.ReadAllText workloadPath).RootElement.Clone()
           candidate =
               {| commit = candidateCommit
                  headTree = headTree
                  workingTreeState = workingTreeState
                  workingTreeDiffSha256 = workingTreeDiffDigest
                  performanceAssemblySha256 = sha256File (Assembly.GetExecutingAssembly().Location)
                  simulationAssemblySha256 = sha256File typeof<SimulationState>.Assembly.Location |}
           host = {| operatingSystem = RuntimeInformation.OSDescription; architecture = RuntimeInformation.ProcessArchitecture.ToString(); framework = RuntimeInformation.FrameworkDescription; runtimeVersion = Environment.Version.ToString(); processorCount = Environment.ProcessorCount; processorModel = processorModel (); gcServer = System.Runtime.GCSettings.IsServerGC; gcLatencyMode = System.Runtime.GCSettings.LatencyMode.ToString() |}
           observation = {| p95Milliseconds = stressP95; worstMilliseconds = stressWorst; units = state.Units.Count; ticks = state.Tick; candidatePairs = 20000; servicedSlots = totalSlots; losEvaluations = totalLos; moves = totalMoves; engagementObservations = totalEngagements; reactionCandidates = totalReactions; evidenceBytes = evidenceBytes; maximumAllocation = maximumAllocation |} |}
    Path.GetDirectoryName(Path.GetFullPath receiptPath) |> Option.ofObj |> Option.iter (Directory.CreateDirectory >> ignore)
    File.WriteAllText(receiptPath, JsonSerializer.Serialize(receipt, JsonSerializerOptions(WriteIndented = true)) + Environment.NewLine)
    printfn "receipt=%s workload-sha256=%s" receiptPath workloadDigest
    if passed then 0 else 1

// ---------------------------------------------------------------------------------------------
// #249 AC 7 - per-finding working-set measurement on a WALL-DENSE fixture.
//
// The spatial performance fixture that already exists runs with `Boundaries = []`, so the
// boundary-index finding (F2) is invisible there BY CONSTRUCTION - there is nothing to look up.
// Each workload below isolates one finding on a walled map, and each prints a deterministic
// structural counter beside its timing so a reader can separate a real change from timing noise.
// Run the same binary against the pre-change implementation to get the "before" side; the harness
// touches only public API, so it compiles unchanged against both.
// ---------------------------------------------------------------------------------------------

let private workingSetIdentity revision =
    SpatialAuthorityIdentity.create "working-set-map" "working-set-rules" revision "working-set-authority" revision
    |> Result.defaultWith failwith

let private wallDenseBoundaries size groundOpen =
    [ for row in 0 .. size - 1 do
        for col in 0 .. size - 2 do
            if (col + row) % 3 = 0 then
                match Edges.edgeBetween (cell (int32 col) (int32 row)) (cell (int32 col + 1) (int32 row)) with
                | Some edge ->
                    yield
                        { Edge = edge
                          Permeability = { Ground = groundOpen; Vision = true; Projectile = true }
                          RevisionToken = sprintf "edge:%d:%d" col row }
                | None -> () ]

let private wallDenseWorld size groundOpen discloseTokens =
    let boundaries = wallDenseBoundaries size groundOpen
    { Identity = workingSetIdentity 1L
      Minimum = cell 0 0
      Maximum = cell (int32 size - 1) (int32 size - 1)
      Terrain = Map.empty
      Boundaries = boundaries
      Occupancy = Map.empty
      DisclosedRevisionTokens =
        if discloseTokens then boundaries |> List.map (fun boundary -> boundary.RevisionToken) |> Set.ofList
        else Set.empty }

let private workingSetRequest id kind modality origin target footprint =
    { QueryId = id
      QueryKind = kind
      Origin = origin
      Target = target
      Footprint = footprint
      Profile =
        { ProfileId = "working-set-v1"
          Modality = modality
          Stance = "standing"
          HeightBand = 1
          Facing = Direction8.North }
      Bounds = SpatialQuery.defaultBounds }

let private squareFootprint side =
    [ for row in 0 .. side - 1 do
        for col in 0 .. side - 1 -> cell (int32 col) (int32 row) ]

let private measureRepeated warmup iterations (action: unit -> 'a) =
    for _ in 1 .. warmup do action () |> ignore
    // Settle the heap between workloads. Without this the workloads contaminate each other: an
    // earlier workload that allocates differently leaves the collector in a different state, and
    // the next measurement moves with THAT rather than with the code under test. It was mistaken
    // for a 60% regression in the cache workload until the same number reproduced with the
    // original code restored.
    GC.Collect()
    GC.WaitForPendingFinalizers()
    GC.Collect()
    let clock = Stopwatch.StartNew()
    let mutable last = action ()
    for _ in 2 .. iterations do last <- action ()
    clock.Stop()
    last, clock.Elapsed.TotalMilliseconds / float iterations

// Each workload runs in its OWN PROCESS, selected by name. They used to run one after another in a
// single process and that made the harness lie: the indexed trace and path workloads allocate a
// boundary index per evaluation, and the heap state they left behind slowed the cache and tick
// workloads that ran after them by roughly 2x - which read as a regression in code those later
// workloads never executed. `GC.Collect` between workloads did not settle it. Isolation did.
let private runWorkingSetMeasurement selected =
    let selects name = selected = "all" || selected = name
    // Which RUNTIME actually loaded. Three lanes on this board have now been bitten by
    // `DOTNET_ROOT_X64`: with it set, two invocation routes can resolve different runtimes, and a
    // before/after comparison across them measures the runtime difference as if it were the code
    // difference. Unlike an assembly digest, a framework version is a MEANINGFUL identity - so the
    // driver records this per side and refuses when the two disagree.
    printfn
        "working-set runtime=%s"
        (System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Replace(" ", "-"))
    // F2 + F5 - exact line of sight over a 4x4 footprint, which is 256 origin/target pairs, on a
    // walled map. F2 is the per-crossing boundary resolution; F5 is the line materialization.
    if selects "f2f5" then
     let traceWorld = wallDenseWorld 64 true false
     let traceRequest =
        workingSetRequest "working-set-los" SpatialQueryKind.ExactLineOfSight SpatialModality.Vision (cell 0 0) (cell 10 10) (squareFootprint 4)
     let traceResult, traceMs = measureRepeated 3 20 (fun () -> SpatialQuery.evaluate traceWorld traceRequest |> fst)
     printfn
        "working-set f2f5-exact-los-ms=%.3f boundaries=%d footprint=%d crossed-cells=%d crossed-edges=%d outcome=%A visible=%b"
        traceMs traceWorld.Boundaries.Length traceRequest.Footprint.Length
        traceResult.Explanation.CrossedCells.Length traceResult.Explanation.CrossedEdges.Length
        traceResult.Outcome traceResult.Visible

    if selects "f4" then
     // F4 - a bounded path whose package A* candidate is REJECTED by the boundary rule, because
    // `anchorPassable` does not consult boundaries and `transitionPassable` does. That is what
    // makes the rewritten fallback loop the path actually taken on a wall-dense map.
     let pathWorld = wallDenseWorld 24 false false
     let pathRequest =
        workingSetRequest "working-set-path" SpatialQueryKind.BoundedPath SpatialModality.GroundMovement (cell 0 0) (cell 20 20) [ cell 0 0 ]
     let pathResult, pathMs = measureRepeated 3 10 (fun () -> SpatialQuery.evaluate pathWorld pathRequest |> fst)
     printfn
        "working-set f4-bounded-path-ms=%.3f boundaries=%d expansions=%d path-cells=%d cost=%d outcome=%A"
        pathMs pathWorld.Boundaries.Length pathResult.Explanation.Expansions
        pathResult.Path.Length pathResult.MovementCost pathResult.Outcome

    if selects "f3" then
     // F3 - lookups against an already-populated dynamic tier. The cost removed is the
    // `DynamicEntries @ StaticEntries` copy that ran on EVERY lookup, hits included.
     let cacheWorld = wallDenseWorld 24 true true
     let cacheRequests =
        [ for index in 0 .. 511 ->
            workingSetRequest (sprintf "working-set-cache-%04d" index) SpatialQueryKind.LineTrace SpatialModality.Vision (cell 0 0) (cell 3 0) [ cell 0 0 ] ]
     let populated =
        cacheRequests
        |> List.fold
            (fun cache request ->
                let _, next, _ = SpatialQuery.evaluateCached cache cacheWorld request
                next)
            SpatialQuery.emptyCache
     let probe = List.head cacheRequests
     let _, cacheMs = measureRepeated 100 2000 (fun () -> SpatialQuery.evaluateCached populated cacheWorld probe)
     printfn
        "working-set f3-cache-lookup-us=%.4f dynamic-entries=%d static-entries=%d"
        (cacheMs * 1000.0) populated.DynamicEntries.Length populated.StaticEntries.Length

    if selects "f1f6" then
     // F1 + F6 - one full authoritative tick whose board carries many semantic edges and whose
    // journal carries many observations. F1 is the per-observation world rebuild; F6 is the
    // duplicated guarded-edge board scan.
     let observationBoard =
        { Minimum = cell 0 0
          Maximum = cell 63 63
          Edges =
            [ for row in 0 .. 31 do
                for col in 0 .. 30 do
                    if (col + row) % 3 = 0 then
                        match Edges.edgeBetween (cell (int32 col) (int32 row)) (cell (int32 col + 1) (int32 row)) with
                        | Some edge ->
                            yield
                                { EdgeId = sprintf "working-set-edge-%d-%d" col row
                                  SpatialRevision = 1
                                  Edge = edge
                                  BlocksMovement = (col + row) % 6 = 0 }
                        | None -> () ]
          Covers = Map.empty }
     let observationState = { state 40 8 8 [] with Board = observationBoard }
     let observationJournal =
        [ for observer in 1 .. 20 -> KernelInput.Observe(Simulation.unitId (int32 observer), Simulation.unitId (int32 observer + 20)) ]
     let tickResult, tickMs = measureRepeated 3 10 (fun () -> Simulation.runTick observationState observationJournal)
     printfn
        "working-set f1f6-observation-tick-ms=%.3f board-edges=%d observations=%d observed=%d events=%d"
        tickMs observationBoard.Edges.Length observationJournal.Length
        tickResult.State.Observations.Count tickResult.Events.Length
    0

exception AwarenessPerformanceExit of int

[<EntryPoint>]
let main args =
    try
        if args |> Array.contains "--working-set" then
            let selected =
                args
                |> Array.tryFindIndex (fun value -> value = "--working-set")
                |> Option.bind (fun index -> args |> Array.tryItem (index + 1))
                |> Option.filter (fun value -> not (value.StartsWith "--"))
                |> Option.defaultValue "all"
            raise (AwarenessPerformanceExit(runWorkingSetMeasurement selected))
        elif args |> Array.contains "--verify-awareness-receipt" then
            raise (AwarenessPerformanceExit(verifyAwarenessPerformanceReceipt (argument "--verify-awareness-receipt" args) (argument "--candidate-commit" args)))
        elif args |> Array.contains "--awareness" then
            raise (AwarenessPerformanceExit(runAwarenessPerformance ()))
        let receiptPath = argument "--receipt" args
        let candidateCommit = argument "--candidate-commit" args
        let sourceTreeState = argument "--source-tree-state" args
        let workloadDefinition = argument "--workload-definition" args
        let expectedWorkloadDigest = argument "--workload-digest" args
        let mutation = Environment.GetEnvironmentVariable("SIR_COMBAT_PERF_MUTATE_CAP") |> Option.ofObj |> Option.defaultValue ""

        required (candidateCommit.Length = 40 && candidateCommit |> Seq.forall Uri.IsHexDigit) "Candidate commit identity must be an exact 40-character SHA."
        required (sourceTreeState = "clean" || sourceTreeState = "dirty-development") "Source-tree state must be an explicit supported value."
        required (validSha expectedWorkloadDigest) "Workload definition identity must be an exact SHA-256 digest."
        required (File.Exists workloadDefinition) "The workload definition is unreadable."
        required (sha256File workloadDefinition = expectedWorkloadDigest.ToLowerInvariant()) "The workload definition digest is stale."

        let representative = execute "representative-authoritative-tick" 3 5 (representativeWorkload ())
        let stress = execute "stress-authoritative-tick" 3 5 (stressWorkload ())
        let maximumObserved = maximum representative.MaximumObserved stress.MaximumObserved
        let limits = mutatedLimits mutation maximumObserved
        let identity = CombatRules.packageIdentity
        let performanceAssembly = Assembly.GetExecutingAssembly().Location
        let simulationAssembly = typeof<SimulationState>.Assembly.Location

        let passed =
            representative.P95Milliseconds <= 20L
            && stress.P95Milliseconds <= 50L
            && representative.Units = 25
            && representative.AttacksRequested = 12
            && stress.Units = 100
            && stress.AttacksRequested = 50
            && within limits representative.MaximumObserved
            && within limits stress.MaximumObserved

        let receipt =
            { Schema = "sir.physical-combat.performance-receipt/1"
              Outcome = if passed then "pass" else "fail"
              ProductionRoute = "SIR.Simulation.Simulation.runPhysicalTickWithRules(Simulation.defaultRules)"
              BuildConfiguration = "Release"
              GeneratedAtUtc = DateTimeOffset.UtcNow.ToString("O")
              Identity =
                { CandidateCommit = candidateCommit.ToLowerInvariant()
                  SourceTreeState = sourceTreeState
                  WorkloadDefinitionSha256 = expectedWorkloadDigest.ToLowerInvariant()
                  PerformanceAssemblySha256 = sha256File performanceAssembly
                  SimulationAssemblySha256 = sha256File simulationAssembly
                  RulesEngine = identity.EngineIdentity
                  RulesCompatibilityProfile = identity.CompatibilityProfile
                  RulesPackageVersion = identity.PackageVersion
                  RulesSourceCommit = identity.SourceCommit
                  RulesImplementationSha256 = hex identity.ImplementationDigest
                  RulesSemanticSha256 = hex identity.SemanticDigest
                  RulesManifestSha256 = hex identity.ManifestDigest }
              Host =
                { OperatingSystem = RuntimeInformation.OSDescription
                  Architecture = RuntimeInformation.ProcessArchitecture.ToString()
                  Framework = RuntimeInformation.FrameworkDescription
                  RuntimeVersion = Environment.Version.ToString()
                  ProcessorCount = Environment.ProcessorCount
                  ProcessorModel = processorModel ()
                  MachineName = Environment.MachineName }
              Limits = limits
              Representative = representative
              Stress = stress
              Mutation = mutation }

        let options = JsonSerializerOptions(WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        Path.GetDirectoryName(Path.GetFullPath receiptPath)
        |> Option.ofObj
        |> Option.iter (Directory.CreateDirectory >> ignore)
        File.WriteAllText(receiptPath, JsonSerializer.Serialize(receipt, options) + Environment.NewLine)
        printfn "receipt=%s" receiptPath
        printfn "representative-p95-ms=%d/20 stress-p95-ms=%d/50" representative.P95Milliseconds stress.P95Milliseconds
        printfn "observed trace=%d area=%d recipients=%d facts=%d evidence-bytes=%d" maximumObserved.TraceCells maximumObserved.AreaCells maximumObserved.Recipients maximumObserved.Facts maximumObserved.CanonicalEvidenceBytes

        if passed then 0 else 1
    with
    | AwarenessPerformanceExit code -> code
    | error ->
        eprintfn "Physical combat performance qualification failed: %s" error.Message
        2
