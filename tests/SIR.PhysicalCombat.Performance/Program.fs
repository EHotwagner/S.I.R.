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
        [ { CoverId = "representative-soft-cover"; Cell = cell 2 2; Integrity = 100; ProjectileBlocking = false }
          { CoverId = "representative-hard-cover"; Cell = cell 3 3; Integrity = 100; ProjectileBlocking = true } ]

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
                  LastKnownCell = Some(cell (int32 (20 + (index % 10) * 3)) (int32 ((index / 10) * 3)))
                  RetainUntilTick = Some AwarenessReaction.infantryProfile.LastKnownRetentionTicks
                  Reason = AwarenessReason.IdentificationThresholdReached } ]
        |> Map.ofList
    { Tick = 0
      Board = { Minimum = cell 0 0; Maximum = cell 79 79; Edges = []; Covers = Map.empty }
      Units = reds @ blues |> Map.ofList
      Observations = Set.empty
      Awareness = acquired
      Engagements = Map.empty
      AwarenessCursor = 0 }

let private runAwarenessPerformance () =
    let mutation = Environment.GetEnvironmentVariable("SIR_AWARENESS_PERF_MUTATE_SUBJECT") |> Option.ofObj |> Option.defaultValue ""
    let workloadPath = "work/182-awareness-reaction-windows/contracts/awareness-reaction-performance-workload-v1.json"
    use workload = JsonDocument.Parse(File.ReadAllText workloadPath)
    required (workload.RootElement.GetProperty("workloadId").GetString() = "sir-awareness-reaction-authoritative-tick-v1") "Awareness workload identity is unreadable."
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
        totalSlots <- totalSlots + int64 result.AwarenessCounters.AwarenessEpisodes
        totalLos <- totalLos + int64 result.AwarenessCounters.LosEvaluations
        totalMoves <- totalMoves + (result.Events |> List.sumBy (function UnitMoved _ -> 1 | _ -> 0))
        totalEngagements <- totalEngagements + int64 result.AwarenessCounters.Engagements
        totalReactions <- totalReactions + int64 result.AwarenessCounters.ReactionCandidates
        evidenceBytes <- max evidenceBytes result.EventBytes.Length
        if sample > 19 then samples.Add watch.ElapsedMilliseconds
    let sorted = samples |> Seq.sort |> Seq.toArray
    let stressP95 = sorted[int (Math.Ceiling(float sorted.Length * 0.95)) - 1]
    let stressWorst = sorted[sorted.Length - 1]
    let coveredObservers = state.Awareness |> Map.toSeq |> Seq.map (fst >> fst) |> Set.ofSeq |> Set.count
    let passed =
        state.Units.Count = 200
        && state.AwarenessCursor <> 0
        && coveredObservers = 200
        && totalSlots > 20_000L
        && totalLos > 10_000L
        && totalMoves > 0
        && totalEngagements > 0L
        && totalReactions > 0L
        && evidenceBytes <= 262_144
        && maximumAllocation <= 100_000_000L
        && stressWorst <= 50L
    printfn "route=SIR.Simulation.Simulation.runTick"
    printfn "full-tick-p95-ms=%d target=20 hard-ceiling=50 worst-ms=%d/50" stressP95 stressWorst
    printfn "stress-units=%d ticks=%d cursor=%d covered-observers=%d serviced-slots=%d los=%d moves=%d engagement-observations=%d reaction-candidates=%d evidence-bytes=%d/262144 max-allocation=%d/100000000 mutation=%s" state.Units.Count state.Tick state.AwarenessCursor coveredObservers totalSlots totalLos totalMoves totalEngagements totalReactions evidenceBytes maximumAllocation mutation
    if passed then 0 else 1

exception AwarenessPerformanceExit of int

[<EntryPoint>]
let main args =
    try
        if args |> Array.contains "--awareness" then
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
