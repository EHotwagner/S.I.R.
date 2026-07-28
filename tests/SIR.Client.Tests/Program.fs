module SIR.Client.Tests

open System
open System.IO
open SIR.Client
open SIR.Domain

let private require condition message =
    if not condition then failwith message

let private operationFrom effects =
    effects
    |> List.choose (function
        | Run(operation, Cancel) -> None
        | Run(operation, _) -> Some operation)
    |> List.exactlyOne

let private metadata kind =
    { SourceName = "fixture.sirr"
      SourceIdentity = "fixture"
      EngineIdentity = "engine"
      FinalTick = 20
      Kind = kind }

let private projection tick =
    { Tick = tick
      BoardMinimumColumn = 0
      BoardMinimumRow = 0
      BoardMaximumColumn = 2
      BoardMaximumRow = 1
      Units = []
      Events = []
      Checkpoints = []
      PerspectiveHash = None }

[<EntryPoint>]
let main _ =
    let retainedPackage: SIR.Simulation.ReplayPackage =
        { FormatVersion = int32 SIR.Simulation.Replay.CurrentFormatVersion
          EngineHash = EngineCatalog.Current.EngineHash
          RulesetHash = CanonicalHash.sha256 [| 1uy |]
          FullReplayAuthorized = false
          Content = SIR.Simulation.PerspectivePlayback [] }

    let decodedRetainedFixture =
        retainedPackage
        |> SIR.Simulation.Replay.encode
        |> SIR.Simulation.Replay.decode SIR.Simulation.Replay.defaultLimits
        |> Result.defaultWith (fun error ->
            failwithf "The retained v1 fixture did not decode: %A" error)

    require
        (EngineCatalog.tryFind decodedRetainedFixture = Some EngineCatalog.Current)
        "A retained replay did not select its exact engine bundle."

    let missingPackage =
        { retainedPackage with
            EngineHash = CanonicalHash.sha256 [| 2uy |] }

    require
        (EngineCatalog.tryFind missingPackage = None)
        "An unavailable engine silently selected a retained bundle."

    let initial = Shell.init ()
    let loading, effects =
        Shell.update (ReplayBytesSelected("fixture.sirr", [| 1uy |])) initial

    require (loading.Verification = Loading) "Replay selection must enter Loading."
    let firstOperation = operationFrom effects

    let superseded, replacementEffects =
        Shell.update
            (ReplayBytesSelected("replacement.sirr", [| 2uy |]))
            loading

    let replacementOperation = operationFrom replacementEffects
    require
        (replacementOperation <> firstOperation)
        "Replacing a load reused its operation identity."

    let stale, staleEffects =
        Shell.update
            (RunnerResponded(
                firstOperation,
                RunnerFailed "stale"
            ))
            superseded

    require
        (stale = superseded && List.isEmpty staleEffects)
        "Stale response changed the model."

    let verified =
        Shell.update
            (RunnerResponded(
                replacementOperation,
                LoadedPackage(metadata FullReplay, KernelVerified, projection 0)
            ))
            superseded
        |> fst

    require
        (verified.Mode = VerifiedReplay
         && verified.Verification = BrowserKernelVerified)
        "Full replay did not become browser-kernel verified."

    let perspective =
        let pending, pendingEffects =
            Shell.update (ReplayBytesSelected("perspective.sirr", [| 2uy |])) initial

        let operation = operationFrom pendingEffects
        Shell.update
            (RunnerResponded(
                operation,
                LoadedPackage(metadata PerspectiveReplay, ProjectionOnly, projection 0)
            ))
            pending
        |> fst

    require
        (perspective.Mode = PerspectivePlayback
         && perspective.Verification = PerspectiveReady)
        "Perspective package was not kept projection-only."

    let sandbox, forkEffects =
        Shell.update (ParameterEdited("attack-power", 30)) verified

    require
        (match sandbox.Mode, sandbox.Verification with
         | SandboxFork identity, SandboxDerived verificationIdentity ->
             identity = verificationIdentity
         | _ -> false)
        "Parameter edit did not irreversibly create a sandbox identity."
    require (not (List.isEmpty forkEffects)) "Sandbox edit did not request a runner fork."

    let cancelled, cancelEffects = Shell.update CancelRequested sandbox
    require (Option.isNone cancelled.ActiveOperation) "Cancel retained an active operation."
    require
        (cancelEffects
         |> List.exists (function
             | Run(_, Cancel) -> true
             | _ -> false))
        "Cancel did not request runner cancellation."

    let unsupported =
        let pending, pendingEffects =
            Shell.update (ReplayBytesSelected("old.sirr", [| 3uy |])) initial

        let operation = operationFrom pendingEffects
        Shell.update
            (RunnerResponded(operation, RunnerUnsupported "engine unavailable"))
            pending
        |> fst

    require
        (unsupported.Verification = Unsupported "engine unavailable")
        "Unsupported replay state was not preserved."
    require
        (unsupported.Source = Rejected("old.sirr", "engine unavailable"))
        "Unsupported replay did not retain its rejected source."

    let divergent =
        let pending, pendingEffects =
            Shell.update (ReplayBytesSelected("bad.sirr", [| 4uy |])) initial

        let operation = operationFrom pendingEffects
        Shell.update
            (RunnerResponded(
                operation,
                RunnerDiverged(7, "attack", "state hash")
            ))
            pending
        |> fst

    require
        (divergent.Verification = Diverged(7, "attack", "state hash"))
        "Divergence detail was not preserved."
    require
        (match divergent.Source with
         | Rejected("bad.sirr", reason) ->
             reason.Contains("tick 7") && reason.Contains("attack")
         | _ -> false)
        "Divergent replay did not retain its rejected source."

    let deterministicLeft = Shell.update (SpeedChanged Double) verified
    let deterministicRight = Shell.update (SpeedChanged Double) verified
    require
        (deterministicLeft = deterministicRight)
        "Equal messages and models produced different states or effects."

    let requestLeft = Shell.update StepForward verified
    let requestRight = Shell.update StepForward verified
    require
        (requestLeft = requestRight)
        "Equal runner requests produced different operation identities or effects."

    let longReplay =
        { verified with
            Playback =
                { verified.Playback with
                    CurrentTick = 0
                    FinalTick = 24_000
                    IsPlaying = true } }

    let advancing, advanceEffects = Shell.playbackTick longReplay
    let advanceOperation = operationFrom advanceEffects
    let progressProjection = projection 256

    let progressing =
        Shell.update
            (RunnerResponded(
                advanceOperation,
                RunnerProgress(256, 1, progressProjection)
            ))
            advancing
        |> fst

    require
        (progressing.ActiveOperation = Some advanceOperation
         && progressing.Playback.CurrentTick = 256
         && progressing.Inspection = Some progressProjection
         && progressing.Worker = WorkerBusy 1)
        "Streaming progress completed the operation or copied the wrong projection."

    let batchEnds = WorkerProtocol.batchEnds 0 24_000
    require
        (List.length batchEnds = 94 && List.last batchEnds = 24_000)
        "A normal-length replay was not divided into the expected bounded batches."
    require
        (List.length batchEnds < 24_000)
        "A normal-length replay still requires one render per tick."

    let workerStopped = Shell.update (WorkerTerminated "test crash") verified |> fst
    require
        (workerStopped.Verification = Failed "worker stopped: test crash"
         && workerStopped.Worker = WorkerStopped "test crash"
         && not workerStopped.Playback.IsPlaying)
        "Worker termination left the shell in a verified or playing state."

    let scenario =
        Lab.tryScenario "adjacent-duel"
        |> Option.defaultWith (fun () -> failwith "The adjacent duel scenario is missing.")

    require
        (Lab.catalog.Length = 6
         && (Lab.catalog |> List.map _.Identity |> Set.ofList |> Set.count) = 6)
        "The interactive scenario gallery is incomplete or has duplicate identities."

    require
        (RulesCatalog.unitRoles.Length = 11
         && RulesCatalog.bodyProfiles.Length = 3
         && RulesCatalog.perkProfiles.Length = 42
         && RulesCatalog.weaponRoles.Length = 7
         && RulesCatalog.weaponProfiles.Length = 5
         && RulesCatalog.armorProfiles.Length = 3
         && RulesCatalog.equipmentGroups.Length = 11)
        "The inspectable unit, perk, weapon, armor, or equipment catalog is incomplete."

    let defaultResults =
        Lab.catalog
        |> List.map (fun candidate ->
            Lab.run candidate Map.empty None
            |> Result.defaultWith failwith
            |> fun report -> report.Comparison.Baseline.ResultIdentity)
        |> Set.ofList

    require
        (defaultResults.Count = Lab.catalog.Length)
        "The scenario gallery contains indistinguishable default experiments."

    let baselineReport =
        Lab.run scenario Map.empty None
        |> Result.defaultWith failwith

    require
        (Lab.attackFrames baselineReport = [ 0, 100; 1, 75; 2, 50; 3, 25; 4, 0 ])
        "The visible attack-sequence simulation does not match the canonical result."

    let forkReport =
        Lab.run scenario (Map.ofList [ ("attack-power", 30) ]) (Some "attack-power")
        |> Result.defaultWith failwith

    require
        (baselineReport.Comparison.Baseline = forkReport.Comparison.Baseline)
        "Changing a parameter mutated the fixed baseline."
    require
        (forkReport.Comparison.Fork.Input.EngineIdentity = scenario.EngineIdentity
         && forkReport.Comparison.Fork.Input.RulesetIdentity = scenario.RulesetIdentity
         && forkReport.Comparison.Fork.Input.Parameters
            = Map.ofList [ ("attack-count", 4); ("attack-power", 30) ])
        "A laboratory result omitted its exact inputs or compatibility identities."
    require
        (forkReport.Comparison.Fork.Metrics = Map.ofList [
            ("attack-events", 4)
            ("remaining-health", 0)
            ("total-damage", 100)
        ])
        "The integer experiment metrics changed unexpectedly."
    require
        (forkReport.EvidenceLabel.Contains("not accepted balance"))
        "Exploratory evidence was not separated from accepted balance."

    let repeated =
        Lab.run scenario (Map.ofList [ ("attack-power", 30) ]) (Some "attack-power")
        |> Result.defaultWith failwith

    require (repeated = forkReport) "Equal experiment inputs produced different results."
    require
        (forkReport.Sweep
         |> Option.exists (fun sweep ->
             sweep.Parameter = "attack-power"
             && List.length sweep.Results = 100))
        "The deterministic sweep did not cover the typed parameter domain."

    require
        (match Lab.run scenario (Map.ofList [ ("attack-power", 101) ]) None with
         | Error error -> error.Contains("between 1 and 100")
         | Ok _ -> false)
        "Out-of-range laboratory input passed validation."

    let promotedReport =
        Lab.run scenario (Map.ofList [ ("attack-power", 30) ]) None
        |> Result.defaultWith failwith

    let export = Lab.export promotedReport
    let fixturePath =
        Path.Combine(AppContext.BaseDirectory, "fixtures", "attack-power-30.sir-lab")
    let fixture = File.ReadAllText fixturePath

    require (export = fixture) "The promoted experiment fixture no longer matches its export."
    require
        (export.Contains("format=" + Lab.ExportFormat)
         && export.Contains("fork.parameter.attack-power=30")
         && export.Contains("fork.engine=" + scenario.EngineIdentity))
        "The experiment export is missing reproducibility fields."

    let scenarioLoading, scenarioEffects =
        Shell.update (ScenarioSelected "adjacent-duel") initial
    let scenarioOperation = operationFrom scenarioEffects

    let scenarioReady =
        Shell.update
            (RunnerResponded(
                scenarioOperation,
                LoadedScenario(
                    { metadata DesignScenario with
                        SourceName = "adjacent-duel.sir-scenario"
                        SourceIdentity = baselineReport.Comparison.Baseline.ResultIdentity
                        FinalTick = 1 },
                    scenario,
                    baselineReport,
                    projection 0
                )
            ))
            scenarioLoading
        |> fst

    require
        (scenarioReady.Mode = ScenarioSandbox baselineReport.Comparison.Baseline.ResultIdentity
         && scenarioReady.Lab.Report = Some baselineReport)
        "A selected design scenario did not enter the scenario sandbox."

    let editedScenario, experimentEffects =
        Shell.update (ParameterEdited("attack-power", 30)) scenarioReady
    let experimentOperation = operationFrom experimentEffects

    let compared =
        Shell.update
            (RunnerResponded(
                experimentOperation,
                ExperimentCompleted(
                    forkReport.Comparison.Fork.ResultIdentity,
                    forkReport
                )
            ))
            editedScenario
        |> fst

    require
        (compared.Lab.Report = Some forkReport
         && compared.Verification
            = SandboxDerived forkReport.Comparison.Fork.ResultIdentity)
        "The worker experiment did not replace the comparison with a derived result."

    printfn "Elmish and laboratory tests passed: deterministic update, modes, bounded worker batches, compact progress, failure revocation, immutable baseline, typed validation, deterministic sweep, reproducible fixture export, sandbox, stale responses, and cancellation."
    0
