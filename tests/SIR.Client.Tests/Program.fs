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

let private requestFrom effects =
    effects
    |> List.choose (function
        | Run(_, Cancel) -> None
        | Run(_, request) -> Some request)
    |> List.exactlyOne

let private metadata kind : ReplayMetadata =
    { SourceName = "fixture.sirr"
      SourceIdentity = "fixture"
      EngineIdentity = "engine"
      FinalTick = 20
      Kind = kind }

let private projection tick : InspectionProjection =
    { Tick = tick
      BoardMinimumColumn = 0
      BoardMinimumRow = 0
      BoardMaximumColumn = 2
      BoardMaximumRow = 1
      Units = []
      Edges = []
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
                LoadedPackage(
                    metadata FullReplay
                    |> WorkerTransport.metadataToTransport,
                    KernelVerified,
                    projection 0
                    |> WorkerTransport.inspectionToTransport
                )
            ))
            superseded
        |> fst

    require
        (verified.Mode = VerifiedReplay
         && verified.Verification = BrowserKernelVerified)
        "Full replay did not become browser-kernel verified."

    let disclosedProjection =
        { projection 0 with
            Units =
                [ { Id = 10
                    Side = "Red"
                    Column = 1
                    Row = 0
                    Health = 75
                    HealthMaximum = 100 } ]
            Edges =
                [ { Id = "edge-0"
                    Kind = "wall"
                    State = "solid"
                    StartColumn = 1
                    StartRow = 0
                    EndColumn = 2
                    EndRow = 0 } ]
            Events =
                [ { Id = 7
                    Tick = 4
                    Source = "Accepted WASM output"
                    Summary = "unit 10 attacks unit 20"
                    SourceUnitId = Some 10
                    TargetUnitId = Some 20 } ]
            Checkpoints =
                [ { Tick = 0
                    StateHash = "state"
                    EventHash = "event" } ] }

    require
        (disclosedProjection
         |> WorkerTransport.inspectionToTransport
         |> WorkerTransport.inspectionFromTransport
         |> (=) disclosedProjection)
        "Edges or event links did not survive the bounded worker transport."

    let disclosedModel =
        { verified with
            Inspection = Some disclosedProjection
            Selection =
                { Unit = Some 10
                  Event = Some 7
                  Formula = None } }

    let disclosedFrame =
        Shell.renderFrame disclosedModel
        |> Option.defaultWith (fun () -> failwith "Verified projection did not produce a render frame.")

    require
        (disclosedFrame.Tick = 0
         && disclosedFrame.Disclosure = FullReplayDisclosure
         && disclosedFrame.Units.Length = 1
         && disclosedFrame.Edges.Length = 1
         && disclosedFrame.Events[0].SourceUnitId = Disclosed 10)
        "The bounded full-replay projection was not connected to the render contract."

    let perspective =
        let pending, pendingEffects =
            Shell.update (ReplayBytesSelected("perspective.sirr", [| 2uy |])) initial

        let operation = operationFrom pendingEffects
        Shell.update
            (RunnerResponded(
                operation,
                LoadedPackage(
                    metadata PerspectiveReplay
                    |> WorkerTransport.metadataToTransport,
                    ProjectionOnly,
                    projection 0
                    |> WorkerTransport.inspectionToTransport
                )
            ))
            pending
        |> fst

    require
        (perspective.Mode = PerspectivePlayback
         && perspective.Verification = PerspectiveReady)
        "Perspective package was not kept projection-only."

    let perspectiveFrame =
        Shell.renderFrame perspective
        |> Option.defaultWith (fun () -> failwith "Perspective projection did not produce a frame.")
    require
        (perspectiveFrame.Disclosure = PerspectiveDisclosure
         && perspectiveFrame.Units.Length = 0)
        "Perspective playback invented a unit outside its disclosed source."

    let backward, backwardEffects =
        Shell.update
            StepBackward
            { disclosedModel with
                Playback =
                    { disclosedModel.Playback with
                        CurrentTick = 8 } }
    require
        (requestFrom backwardEffects = Seek(7, disclosedModel.Playback.FinalTick)
         && not backward.Playback.IsPlaying)
        "Backward stepping did not seek one exact committed tick."

    let eventNavigation, eventEffects =
        Shell.update
            NextEvent
            { disclosedModel with
                Playback =
                    { disclosedModel.Playback with
                        CurrentTick = 0 }
                Selection = { disclosedModel.Selection with Event = None } }
    require
        (requestFrom eventEffects = Seek(4, disclosedModel.Playback.FinalTick)
         && eventNavigation.Selection.Event = Some 7)
        "Next-event navigation did not seek and select the disclosed event."

    let seekOperation = operationFrom eventEffects
    let lostContactProjection =
        { disclosedProjection with
            Tick = 4
            Units = []
            Events = [] }
    let mismatchedProgress =
        Shell.update
            (RunnerResponded(
                seekOperation,
                RunnerProgress(
                    3,
                    1,
                    lostContactProjection
                    |> WorkerTransport.inspectionToTransport
                )
            ))
            eventNavigation
        |> fst

    require
        (mismatchedProgress.Playback.CurrentTick = 4
         && mismatchedProgress.Inspection = Some lostContactProjection
         && mismatchedProgress.ActiveOperation = Some seekOperation
         && mismatchedProgress.Announcement.Contains("tick 4"))
        "Progress displayed the response tick instead of its projection's committed tick."

    let lostContact =
        Shell.update
            (RunnerResponded(
                seekOperation,
                Progressed(
                    9,
                    lostContactProjection
                    |> WorkerTransport.inspectionToTransport
                )
            ))
            mismatchedProgress
        |> fst

    require
        (lostContact.Playback.CurrentTick = 4
         && lostContact.Selection.Unit.IsNone
         && lostContact.Selection.Event.IsNone
         && (Shell.renderFrame lostContact
             |> Option.exists (fun frame ->
                 frame.Tick = 4
                 && frame.Units.Length = 0
                 && frame.Events.Length = 0)))
        "Lost contact retained selection/event residue or accepted a non-projection tick."

    let reconciledView =
        Battlefield.reconcile
            (Shell.renderFrame lostContact |> Option.get)
            { Battlefield.initial with
                SelectedUnit = Some 10
                FocusedUnit = Some 10 }
    require
        (reconciledView.SelectedUnit.IsNone && reconciledView.FocusedUnit.IsNone)
        "Lost contact retained SVG selection or roving-focus state."

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
                RunnerProgress(
                    256,
                    1,
                    progressProjection
                    |> WorkerTransport.inspectionToTransport
                )
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

    require
        (baselineReport
         |> Lab.reportToTransport
         |> Lab.reportFromTransport
         |> (=) baselineReport)
        "The structured-clone-safe report transport does not round-trip."

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
                        FinalTick = 1 }
                    |> WorkerTransport.metadataToTransport,
                    Lab.scenarioToTransport scenario,
                    Lab.reportToTransport baselineReport,
                    projection 0
                    |> WorkerTransport.inspectionToTransport
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
                    Lab.reportToTransport forkReport
                )
            ))
            editedScenario
        |> fst

    require
        (compared.Lab.Report = Some forkReport
         && compared.Verification
            = SandboxDerived forkReport.Comparison.Fork.ResultIdentity)
        "The worker experiment did not replace the comparison with a derived result."

    let extent =
        CellExtent.tryCreate 1
        |> Option.defaultWith (fun () -> failwith "A one-cell extent was rejected.")

    let heading =
        HeadingRadians.tryCreate (-Math.PI / 2.0)
        |> Option.defaultWith (fun () -> failwith "A finite heading was rejected.")

    let health =
        HealthVisual.tryCreate 75 100
        |> Option.defaultWith (fun () -> failwith "Valid health was rejected.")

    require
        (HeadingRadians.value heading >= 0.0
         && HeadingRadians.value heading < Math.PI * 2.0
         && HeadingRadians.tryCreate Double.NaN = None
         && CellExtent.tryCreate 0 = None
         && HealthVisual.tryCreate 101 100 = None)
        "Presentation contract constraints accepted invalid values."

    let renderFrame: RenderFrame =
        { Tick = 7
          Board =
            { MinimumColumn = 0
              MinimumRow = 0
              MaximumColumn = 10
              MaximumRow = 8 }
          Units =
            [| { Id = 42
                 AnchorColumn = 3
                 AnchorRow = 4
                 FootprintWidth = extent
                 FootprintDepth = extent
                 ClassId = UnitClassId.resolve "rifleman"
                 Faction = Human
                 Health = Disclosed health
                 Level = NotPresent
                 StanceId = NotApplicable
                 BodyHeading = Disclosed heading
                 SecondaryHeading =
                    Disclosed
                        { Radians = heading
                          Source = WeaponHeading }
                 ShortLabel = Disclosed "Bravo 6"
                 StatusIds = [| "suppressed" |] } |]
          Edges =
            [| { Id = "edge-1"
                 Kind = "door"
                 State = "open"
                 StartColumn = 2
                 StartRow = 2
                 EndColumn = 3
                 EndRow = 2 } |]
          Overlays =
            [| { Id = "overlay-1"
                 Kind = "visible-polygon"
                 Scope = SelectedUnitOverlay 42
                 GeometryRevision = 2
                 Points = [| 1.0; 2.0; 3.0; 4.0 |]
                 Label = NotPresent } |]
          Events =
            [| { Id = 9
                 Tick = 7
                 Kind = "observation"
                 SourceUnitId = Disclosed 42
                 TargetUnitId = ExplicitlyUnknown
                 Summary = NotPresent } |]
          Disclosure = PerspectiveDisclosure }

    let renderTransport = RenderFrameTransport.toTransport renderFrame
    let renderRoundTrip = RenderFrameTransport.fromTransport renderTransport
    require
        (renderRoundTrip = renderFrame)
        "The render frame did not survive its structured-clone transport."

    let missingSecondarySource =
        { renderTransport with
            Units =
                [| { renderTransport.Units[0] with
                         SecondaryHeadingSource = None } |] }
    require
        (try
             RenderFrameTransport.fromTransport missingSecondarySource |> ignore
             false
         with :? ArgumentException ->
             true)
        "A second heading without an explicitly accepted gameplay source was rendered."

    let malformedDisclosure =
        { renderTransport with
            Units =
                [| { renderTransport.Units[0] with
                         LevelKind = 0
                         Level = Some 0 } |] }

    require
        (try
             RenderFrameTransport.fromTransport malformedDisclosure |> ignore
             false
         with :? ArgumentException ->
             true)
        "An ambiguous disclosure tag/value pair was silently defaulted."

    let expectedCatalogIds =
        [| "rifleman"
           "gunner"
           "marksman"
           "engineer"
           "medic"
           "signaller"
           "observation-drone"
           "relay-drone"
           "goblin"
           "orc"
           "troll"
           "senior-caster"
           "magical-assistant"
           "ambient-critter" |]

    let actualCatalogIds =
        UnitGlyphCatalog.all
        |> Array.map (fun glyph -> UnitClassId.value glyph.Id)

    require
        (Set.ofArray actualCatalogIds = Set.ofArray expectedCatalogIds
         && actualCatalogIds.Length = (Set.ofArray actualCatalogIds).Count)
        "The initial exact-class inventory is missing or has duplicate glyphs."
    require
        (UnitGlyphCatalog.all
         |> Array.forall (fun glyph ->
             not (String.IsNullOrWhiteSpace glyph.Description)
             && not (String.IsNullOrWhiteSpace glyph.TextAlternative)
             && not (Array.isEmpty glyph.Primitives)))
        "A catalog glyph lacks geometry or an accessibility description."
    require
        (UnitClassId.resolve "replay-supplied-markup" = UnitClassId.placeholder
         && UnitGlyphCatalog.resolve (UnitClassId.resolve "replay-supplied-markup")
            = UnitGlyphCatalog.placeholder
         && not (Array.contains "unknown-unit" actualCatalogIds))
        "Unknown class input did not resolve to the distinct safe placeholder."

    let documentedRoleCatalogIds =
        RulesCatalog.unitRoles
        |> List.map (fun role ->
            match role.Name with
            | "Senior caster" -> "senior-caster"
            | "Magical assistant" -> "magical-assistant"
            | name -> name.ToLowerInvariant())
        |> Set.ofList

    require
        (Set.isSubset documentedRoleCatalogIds (Set.ofArray actualCatalogIds))
        "A documented unit role has no exact-class catalog entry."

    let paletteIds =
        ReplayPalettes.all
        |> Array.map _.Id
        |> Set.ofArray

    let expectedPaletteIds =
        Set.ofArray
            [| "accessible-default"
               "high-contrast"
               "monochrome-pattern" |]

    let palettesAreComplete =
        ReplayPalettes.all
        |> Array.forall (fun palette ->
            palette.UsesPatterns
            && palette.OverlayPatterns.Length >= 4
            && not (String.IsNullOrWhiteSpace palette.Focus))

    require
        (paletteIds = expectedPaletteIds
         && palettesAreComplete
         && ReplayPalettes.monochromePattern.HumanFaction
            = ReplayPalettes.monochromePattern.ArcaneFaction)
        "The required accessible palette token sets are incomplete."

    let staticScene =
        Battlefield.scene
            Battlefield.representativeFrame
            Battlefield.initial

    require
        (staticScene.Tick = Battlefield.representativeFrame.Tick
         && staticScene.Width = 384.0
         && staticScene.Height = 384.0
         && staticScene.SemanticZoom = Detailed)
        "The representative committed frame was changed or interpolated."

    require
        (staticScene.Units
         |> Array.filter (fun unit -> unit.Unit.Faction = Human)
         |> Array.forall (fun unit ->
             CellExtent.value unit.Unit.FootprintWidth = 2
             && CellExtent.value unit.Unit.FootprintDepth = 2))
        "The representative human units no longer use the canonical 2x2 footprint."

    require
        (staticScene.Units
         |> Array.forall (fun unit ->
             unit.FootprintX = float unit.Unit.AnchorColumn * Battlefield.CellSize
             && unit.FootprintY = float unit.Unit.AnchorRow * Battlefield.CellSize
             && unit.FootprintWidth
                = float (CellExtent.value unit.Unit.FootprintWidth)
                  * Battlefield.CellSize
             && unit.FootprintDepth
                = float (CellExtent.value unit.Unit.FootprintDepth)
                  * Battlefield.CellSize
             && unit.Unit.FootprintWidth = unit.Unit.FootprintDepth
             && unit.FootprintWidth = unit.FootprintDepth))
        "The top-down projection did not preserve authoritative footprint cells."

    let offsetFrame =
        let source = Battlefield.representativeFrame.Units[0]
        { Battlefield.representativeFrame with
            Board =
                { MinimumColumn = 10
                  MinimumRow = 20
                  MaximumColumn = 12
                  MaximumRow = 21 }
            Units =
                [| { source with
                       AnchorColumn = 10
                       AnchorRow = 20 } |]
            Edges =
                [| { Id = "offset-edge"
                     Kind = "wall"
                     State = "solid"
                     StartColumn = 10
                     StartRow = 20
                     EndColumn = 12
                     EndRow = 20 } |] }
    let offsetScene = Battlefield.scene offsetFrame Battlefield.initial
    require
        (offsetScene.Width = 144.0
         && offsetScene.Height = 96.0
         && offsetScene.Units[0].FootprintX = 0.0
         && offsetScene.Units[0].FootprintY = 0.0
         && offsetScene.Edges[0].StartColumn = 10)
        "A board with non-zero minimum coordinates was not projected relative to its origin."

    let healthSegments =
        staticScene.Units |> Array.map _.HealthSegments

    require
        (healthSegments = [| Some 12; Some 9; Some 6; Some 11; Some 8; Some 3 |]
         && healthSegments
            |> Array.forall (Option.forall (fun value -> value >= 0 && value <= 12)))
        "The twelve-position health mapping changed or exceeded its bounds."

    let omittedHealthFrame =
        { Battlefield.representativeFrame with
            Units =
                Battlefield.representativeFrame.Units
                |> Array.mapi (fun index unit ->
                    if index = 0 then { unit with Health = NotPresent } else unit) }
    let omittedHealthScene =
        Battlefield.scene omittedHealthFrame Battlefield.initial
    require
        (omittedHealthScene.Units[0].HealthSegments = None
         && omittedHealthScene.Units[0].AccessibleLabel.Contains(
             "health not present in this projection"
         ))
        "Undisclosed health was rendered as a constructed zero."

    let elevated =
        staticScene.Units
        |> Array.find (fun unit -> unit.Unit.Id = 6)

    require
        (elevated.ElevationBars = 3
         && elevated.ElevationLabel = Some "+7"
         && elevated.ShowStance)
        "Capped elevation or detailed stance disclosure was not projected."

    require
        (Battlefield.semanticZoom Overview 26.39 = Overview
         && Battlefield.semanticZoom Overview 26.41 = Standard
         && Battlefield.semanticZoom Standard 21.6 = Standard
         && Battlefield.semanticZoom Standard 21.59 = Overview
         && Battlefield.semanticZoom Standard 52.81 = Detailed
         && Battlefield.semanticZoom Detailed 43.2 = Detailed
         && Battlefield.semanticZoom Detailed 43.19 = Standard)
        "The 24/48 px semantic thresholds lost their ten-percent hysteresis."

    let standardState =
        Battlefield.update
            Battlefield.representativeFrame
            (ZoomBy 0.8)
            Battlefield.initial
    let standardScene =
        Battlefield.scene Battlefield.representativeFrame standardState

    require
        (standardScene.SemanticZoom = Standard
         && standardScene.Units |> Array.forall (fun unit -> not unit.ShowStance)
         && standardScene.Units
            |> Array.forall (fun unit -> Option.isNone unit.ElevationLabel))
        "Standard zoom retained detailed-only stance or elevation labels."

    let focusedRight =
        Battlefield.initial
        |> Battlefield.update Battlefield.representativeFrame (FocusUnit(Some 1))
        |> Battlefield.update Battlefield.representativeFrame (FocusDirection(1, 0))

    require
        (focusedRight.FocusedUnit = Some 2)
        "Roving directional focus did not choose the nearest disclosed unit."

    let paletteScenes =
        ReplayPalettes.all
        |> Array.map (fun palette ->
            Battlefield.initial
            |> Battlefield.update
                Battlefield.representativeFrame
                (ChoosePalette palette.Id)
            |> Battlefield.scene Battlefield.representativeFrame)

    require
        (paletteScenes |> Array.map _.Palette.Id |> Set.ofArray = expectedPaletteIds
         && paletteScenes
            |> Array.forall (fun scene ->
                scene.Units.Length = staticScene.Units.Length
                && scene.Tick = staticScene.Tick))
        "A palette changed committed geometry or failed to render."

    let evidenceLeft = Battlefield.deterministicEvidence staticScene
    let evidenceRight =
        Battlefield.scene Battlefield.representativeFrame Battlefield.initial
        |> Battlefield.deterministicEvidence

    require
        (evidenceLeft = evidenceRight
         && evidenceLeft.Contains("tick=24")
         && evidenceLeft.Contains("unit=3@0,144:96x96")
         && evidenceLeft.Contains("health=12"))
        "Static SVG review evidence is not deterministic."

    require
        (staticScene.Overlays
         |> Array.exists (fun overlay ->
             overlay.Overlay.Id = "selected-los-1"
             && overlay.Disposition = ExactOverlay
             && overlay.PathSegments = 3)
         && staticScene.ActionTraces.Length = 2
         && staticScene.Timeline
            |> Array.map _.Lane
            |> Set.ofArray
            |> Set.isSubset (Set.ofList [ UnitActions; Communications ])
         && staticScene.Units
            |> Array.filter (fun unit ->
                match unit.Unit.SecondaryHeading with
                | Disclosed _ -> true
                | _ -> false)
            |> Array.length
            |> (=) 2)
        "The exact selected overlay, action traces, semantic lanes, or typed two-heading review fixture is incomplete."

    let pointsForSegments segments =
        Array.init ((segments + 1) * 2) (fun coordinate ->
            float (coordinate / 2 % 200))

    let overlayStressFrame =
        { Battlefield.representativeFrame with
            Overlays =
                [| { Id = "selected-exact"
                     Kind = "perception"
                     Scope = SelectedUnitOverlay 1
                     GeometryRevision = 1
                     Points = pointsForSegments 1_999
                     Label = NotPresent }
                   { Id = "whole-force-over-budget"
                     Kind = "command"
                     Scope = WholeForceOverlay
                     GeometryRevision = 1
                     Points = pointsForSegments 8_001
                     Label = NotPresent } |] }
    let overlayStressScene =
        Battlefield.scene overlayStressFrame Battlefield.initial
    let selectedStress =
        overlayStressScene.Overlays
        |> Array.find (fun overlay -> overlay.Overlay.Id = "selected-exact")
    let wholeStress =
        overlayStressScene.Overlays
        |> Array.find (fun overlay ->
            overlay.Overlay.Id = "whole-force-over-budget")
    require
        (selectedStress.Disposition = ExactOverlay
         && selectedStress.PathSegments = 1_999
         && wholeStress.PathSegments = 4
         && wholeStress.Disposition
            = AggregatedWholeForceOverlay 8_001
         && overlayStressScene.WholeForceOverlaySegments = 8_001)
        "Whole-force aggregation above 8,000 segments degraded the precise selected overlay."

    let selectedWarningFrame =
        { overlayStressFrame with
            Overlays =
                [| { overlayStressFrame.Overlays[0] with
                         Points = pointsForSegments 2_001 } |] }
    let selectedWarning =
        Battlefield.scene selectedWarningFrame Battlefield.initial
        |> _.Overlays[0]
    require
        (selectedWarning.PathSegments = Battlefield.SelectedOverlaySegmentLimit
         && selectedWarning.Disposition = SimplifiedSelectedOverlay 2_001)
        "The selected overlay did not apply its independent 2,000-segment warning/simplification policy."

    let invalidOverlayFrame =
        { overlayStressFrame with
            Overlays =
                [| { overlayStressFrame.Overlays[0] with
                         Points = [| 0.0; 0.0; Double.NaN; 1.0 |] } |] }
    require
        (match
            (Battlefield.scene invalidOverlayFrame Battlefield.initial).Overlays[0].Disposition
         with
         | DeclinedUnsafeOverlay _ -> true
         | _ -> false)
        "Non-finite hostile overlay geometry crossed the renderer boundary."

    let movedFrame =
        let units =
            Battlefield.representativeFrame.Units
            |> Array.map (fun unit ->
                if unit.Id = 1 then
                    { unit with AnchorColumn = unit.AnchorColumn + 1 }
                else
                    unit)
        { Battlefield.representativeFrame with
            Tick = Battlefield.representativeFrame.Tick + 1
            Units = units }
    let halfway =
        Battlefield.interpolatedScene
            0.5
            Battlefield.representativeFrame
            movedFrame
            Battlefield.initial
    let committedFromInterpolation =
        Battlefield.interpolatedScene
            1.0
            Battlefield.representativeFrame
            movedFrame
            Battlefield.initial
    let exactCommitted = Battlefield.scene movedFrame Battlefield.initial
    require
        (halfway.Units[0].FootprintX = 24.0
         && committedFromInterpolation = exactCommitted
         && Battlefield.deterministicEvidence committedFromInterpolation
            = Battlefield.deterministicEvidence exactCommitted)
        "Deterministic interpolation failed to converge exactly on the committed frame."

    let blockedSource =
        { Battlefield.representativeFrame with
            Edges =
                [| { Id = "blocking-wall"
                     Kind = "wall"
                     State = "solid"
                     StartColumn = 1
                     StartRow = 0
                     EndColumn = 1
                     EndRow = 1 } |] }
    let blockedTarget =
        { movedFrame with Edges = blockedSource.Edges }
    let blockedHalfway =
        Battlefield.interpolatedScene
            0.5
            blockedSource
            blockedTarget
            Battlefield.initial
    let reducedState =
        Battlefield.update
            Battlefield.representativeFrame
            (ChooseReducedMotion true)
            Battlefield.initial
    require
        (blockedHalfway.Units[0].FootprintX = 0.0
         && reducedState.ReducedMotion
         && not reducedState.ExactTicks
         && (Battlefield.update
                 Battlefield.representativeFrame
                 (ChooseExactTicks true)
                 reducedState).ExactTicks)
        "Blocked-edge or explicit reduced-motion/exact-tick behavior regressed."

    let performanceScene =
        Battlefield.performanceFrame 200
        |> fun frame -> Battlefield.scene frame Battlefield.initial

    require
        (performanceScene.Units.Length = 200
         && performanceScene.InteractiveNodeEstimate < 8_000
         && performanceScene.Units[0..12]
            |> Array.map _.HealthSegments
            |> (=) ([| 0 .. 12 |] |> Array.map Some))
        "The normal 200-unit view exceeds the 8,000 interactive-node budget."

    let divergentBaseline =
        { disclosedProjection with
            Tick = 4
            Units =
                [ { Id = 10
                    Side = "Red"
                    Column = 1
                    Row = 0
                    Health = 75
                    HealthMaximum = 100 } ] }
    let divergentFork =
        { divergentBaseline with
            Units =
                [ { divergentBaseline.Units.Head with
                      Column = 2
                      Health = 50 } ]
            Events =
                [ { Id = 8
                    Tick = 4
                    Source = "sandbox"
                    Summary = "derived event"
                    SourceUnitId = Some 10
                    TargetUnitId = None } ] }
    let divergence =
        Comparison.inspect
            divergentBaseline
            divergentFork
            (Map.ofList [ "remaining-health", -25 ])
    require
        (divergence.FirstDivergentEvent = Some 7
         && divergence.FirstDifferingField
            |> Option.exists (fun field ->
                field.UnitId = Some 10 && field.Field = "column"))
        "The comparison did not deterministically identify its first event and disclosed field divergence."

    let linked =
        Comparison.create
            "source"
            "baseline"
            "fork"
            4
            (Some 10)
            divergence
        |> Comparison.addBookmark 4 "first difference"
        |> Comparison.setView DifferenceOverlay
    require
        (linked.BaselineLabel.Contains("Immutable")
         && linked.ForkLabel.Contains("not verified replay")
         && linked.Tick = 4
         && linked.SelectedUnit = Some 10
         && linked.Bookmarks.Length = 1)
        "Comparison labels, linked state, or bookmarks lost the baseline/fork trust boundary."

    let baselineComparisonFrame = Lab.renderFrame baselineReport.Comparison.Baseline
    let visiblyDifferentReport =
        Lab.run scenario (Map.ofList [ "attack-count", 2 ]) None
        |> Result.defaultWith failwith
    let forkComparisonFrame = Lab.renderFrame visiblyDifferentReport.Comparison.Fork
    require
        (baselineComparisonFrame.Disclosure = SandboxDisclosure
         && forkComparisonFrame.Disclosure = SandboxDisclosure
         && baselineComparisonFrame.Units.Length = 2
         && forkComparisonFrame.Units.Length = 2
         && baselineComparisonFrame.Units[1].Health
            <> forkComparisonFrame.Units[1].Health
         && EvidenceExport.projectionIdentity baselineComparisonFrame
            <> EvidenceExport.projectionIdentity forkComparisonFrame)
        "The split comparison did not preserve distinct, fully hashed sandbox result frames."

    let delimiterFrameLeft =
        { renderFrame with
            Edges =
                [| { renderFrame.Edges[0] with
                         Id = "edge:wall"
                         Kind = "open|north" } |] }
    let delimiterFrameRight =
        { renderFrame with
            Edges =
                [| { renderFrame.Edges[0] with
                         Id = "edge"
                         Kind = "wall:open|north" } |] }
    require
        (EvidenceExport.projectionIdentity delimiterFrameLeft
         <> EvidenceExport.projectionIdentity delimiterFrameRight)
        "Length-prefixed projection identity collided on delimiter-shaped edge fields."

    let hostileProvenance =
        { SourceIdentity = "<script>alert(1)</script> https://host.invalid/x"
          ReplayIdentity = "onclick=steal()"
          ProjectionIdentity = "forged-projection"
          EngineIdentity = "engine\"><foreignObject>"
          RulesetIdentity = Some "url(https://host.invalid)"
          Tick = 999
          Mode = DerivedSimulationEvidence
          PaletteIdentity = "replay-supplied-palette"
          RendererVersion = EvidenceExport.RendererVersion }
    let evidence =
        EvidenceExport.svg
            hostileProvenance
            (Some "<img onerror=steal()> javascript:attack data:text/html")
            renderFrame
    let repeatedEvidence =
        EvidenceExport.svg
            hostileProvenance
            (Some "<img onerror=steal()> javascript:attack data:text/html")
            renderFrame
    require
        (evidence = repeatedEvidence
         && EvidenceExport.isClosedSvg evidence.Svg
         && evidence.Svg.Contains("DERIVED SIMULATION — NOT VERIFIED REPLAY")
         && evidence.Svg.Contains("source=")
         && evidence.Svg.Contains("replay=")
         && evidence.Svg.Contains("projection=")
         && evidence.Svg.Contains("palette=accessible-default")
         && evidence.Svg.Contains("renderer=")
         && evidence.Provenance.Tick = renderFrame.Tick
         && evidence.Provenance.ProjectionIdentity
            = EvidenceExport.projectionIdentity renderFrame
         && evidence.Provenance.PaletteIdentity = ReplayPalettes.accessibleDefault.Id
         && evidence.Provenance.RendererVersion = EvidenceExport.RendererVersion)
        "Deterministic evidence export leaked hostile markup/URLs or omitted provenance and the derived label."

    let editor = MapEditor.initial
    let footprintPresetFixture =
        MapEditor.canonicalFootprintPresets
        |> List.map (fun preset ->
            String.concat
                "|"
                [ preset.Id
                  preset.ClassId
                  string preset.FootprintSize
                  string preset.FootprintSize ])
        |> String.concat "\n"
    let expectedFootprintPresetFixture =
        Path.Combine(
            AppContext.BaseDirectory,
            "fixtures",
            "map-editor-milestone-0-footprints.txt"
        )
        |> File.ReadAllText
        |> fun value -> value.TrimEnd('\r', '\n')
    require
        (footprintPresetFixture = expectedFootprintPresetFixture
         && MapEditor.tryCanonicalFootprintPreset "Goblin" = None
         && (Map.find 1 editor.Map.Units).Size = 2
         && (Map.find 3 editor.Map.Units).Size = 1
         && (Map.find 4 editor.Map.Units).Size = 3)
        "The deterministic canonical footprint preset fixture changed."

    let exportedMap = MapEditor.export editor
    let importedMap =
        MapEditor.tryImport exportedMap
        |> Result.defaultWith failwith
    require
        (importedMap = editor.Map
         && exportedMap = MapEditor.export { editor with Map = importedMap })
        "The versioned map document did not round-trip deterministically."

    let editorStep = MapEditor.step editor
    let manualBefore = Map.find 1 editor.Map.Units
    let manualAfter = Map.find 1 editorStep.Map.Units
    let scriptedBefore = Map.find 2 editor.Map.Units
    let scriptedAfter = Map.find 2 editorStep.Map.Units
    let generalBefore = Map.find 3 editor.Map.Units
    let generalAfter = Map.find 3 editorStep.Map.Units
    require
        (manualAfter.Column = manualBefore.Column
         && manualAfter.Row = manualBefore.Row
         && scriptedAfter.Column = scriptedBefore.Column + 1
         && scriptedAfter.ScriptIndex = 1
         && (generalAfter.Column <> generalBefore.Column
             || generalAfter.Row <> generalBefore.Row)
         && editorStep.Tick = 1)
        "Manual, scripted, and general editor controllers did not resolve deterministically."

    let blockedEditor =
        editor
        |> MapEditor.update (ChooseTool(Paint Blocked))
        |> MapEditor.update (ActivateCell(3, 3))
        |> MapEditor.update (ChooseTool(Place(Blue, "rifleman", 2)))
        |> MapEditor.update (ActivateCell(3, 3))
    require
        (blockedEditor.Map.Units.Count = editor.Map.Units.Count
         && blockedEditor.Validation.IsSome)
        "The editor placed a unit across blocked terrain."

    let occupiedPaint =
        editor
        |> MapEditor.update (ChooseTool(Paint Blocked))
        |> MapEditor.update (ActivateCell(1, 1))
    require
        (MapEditor.terrainAt 1 1 occupiedPaint = Open
         && occupiedPaint.Validation.IsSome)
        "The editor painted blocked terrain under an existing unit."

    let clearedEditor = MapEditor.update ClearMap editor
    require
        (clearedEditor.Map.Units.IsEmpty
         && clearedEditor.SelectedUnit.IsNone)
        "Clearing the map left a stale selected unit."

    let editedUnit =
        editor
        |> MapEditor.update (SetSelectedSide Red)
        |> MapEditor.update (SetSelectedClass "engineer")
        |> MapEditor.update (SetSelectedHealth(8, 16))
    let editedUnitValue = Map.find 1 editedUnit.Map.Units
    require
        (editedUnitValue.Side = Red
         && editedUnitValue.ClassId = "engineer"
         && editedUnitValue.Health = 8
         && editedUnitValue.HealthMaximum = 16)
        "The editor did not apply selected-unit properties."

    let invalidResize = MapEditor.update (SetSelectedSize 40) editor
    require
        ((Map.find 1 invalidResize.Map.Units).Size = 2
         && invalidResize.Validation.IsSome)
        "The editor accepted a selected-unit square that does not fit."

    let wideEdgeMap =
        String.concat
            "\n"
            [ "SIR-MAP 1"
              "size 8 8"
              "edge 2 2 south wall closed"
              "unit 1 blue rifleman 1 1 2 12 12 manual -" ]
        + "\n"
    let edgeBlockedEditor =
        editor
        |> MapEditor.update (LoadMapText wideEdgeMap)
        |> MapEditor.update (SelectEditorUnit(Some 1))
        |> MapEditor.update (MoveSelected South)
    require
        ((Map.find 1 edgeBlockedEditor.Map.Units).Row = 1
         && edgeBlockedEditor.Validation = Some "That move is blocked.")
        "A square unit crossed a blocking edge along part of its leading side."

    let invalidTerrainMap =
        "SIR-MAP 1\nsize 4 4\nterrain 4 0 rough\n"
    require
        (MapEditor.tryImport invalidTerrainMap |> Result.isError)
        "The editor imported terrain outside the map."

    let editorFrame = MapEditor.frame editorStep
    require
        (editorFrame.Disclosure = SandboxDisclosure
         && editorFrame.Units
            |> Array.forall (fun unit ->
                unit.FootprintWidth = unit.FootprintDepth))
        "The editor frame lost sandbox disclosure or square footprints."

    let formatCamera label (camera: BattlefieldCamera) =
        String.Format(
            Globalization.CultureInfo.InvariantCulture,
            "{0}={1:F3}|{2:F3}|{3:F3}",
            label,
            camera.PanX,
            camera.PanY,
            camera.Zoom
        )
    let fitCamera =
        MapEditorWorkspace.fitBoard 960.0 640.0 editor.Map.Width editor.Map.Height
    let pointerBoardBefore =
        MapEditorWorkspace.screenToBoard fitCamera 480.0 320.0
    let zoomCamera =
        MapEditorWorkspace.zoomAt 480.0 320.0 1.25 fitCamera
    let pointerBoardAfter =
        MapEditorWorkspace.screenToBoard zoomCamera 480.0 320.0
    let frameCamera =
        MapEditorWorkspace.frameSelection
            960.0
            640.0
            (MapEditor.selected editor)
            fitCamera
    let hitCell =
        MapEditorWorkspace.tryHitCell
            editor.Map.Width
            editor.Map.Height
            fitCamera
            (fitCamera.PanX + 2.5 * Battlefield.CellSize * fitCamera.Zoom)
            (fitCamera.PanY + 1.5 * Battlefield.CellSize * fitCamera.Zoom)
        |> Option.defaultWith (fun () -> failwith "Expected editor cell hit.")
    let lowZoomCamera =
        { PanX = 0.0
          PanY = 0.0
          Zoom = 0.5 }
    let highZoomCamera =
        { PanX = 0.0
          PanY = 0.0
          Zoom = 3.0 }
    let lowZoomEdge =
        MapEditorWorkspace.tryHitEdge
            editor.Map.Width
            editor.Map.Height
            lowZoomCamera
            MapEditorWorkspace.EdgeTolerancePixels
            56.0
            36.0
        |> Option.defaultWith (fun () -> failwith "Expected low-zoom editor edge hit.")
    let highZoomEdge =
        MapEditorWorkspace.tryHitEdge
            editor.Map.Width
            editor.Map.Height
            highZoomCamera
            MapEditorWorkspace.EdgeTolerancePixels
            296.0
            216.0
        |> Option.defaultWith (fun () -> failwith "Expected high-zoom editor edge hit.")
    let formatEdge label (edge: MapEdgeHit) =
        String.Format(
            Globalization.CultureInfo.InvariantCulture,
            "{0}={1}|{2}|{3}|{4:F3}",
            label,
            edge.Column,
            edge.Row,
            edge.Direction,
            edge.DistancePixels
        )
    let cameraFixture =
        [ formatCamera "fit" fitCamera
          formatCamera "zoom" zoomCamera
          formatCamera "frame" frameCamera
          "cell=" + string hitCell.Column + "|" + string hitCell.Row
          formatEdge "edge-low" lowZoomEdge
          formatEdge "edge-high" highZoomEdge ]
        |> String.concat "\n"
    let expectedCameraFixture =
        Path.Combine(
            AppContext.BaseDirectory,
            "fixtures",
            "map-editor-milestone-1-camera.txt"
        )
        |> File.ReadAllText
        |> fun value -> value.TrimEnd('\r', '\n')
    require
        (cameraFixture = expectedCameraFixture
         && abs (fst pointerBoardBefore - fst pointerBoardAfter) < 0.000_001
         && abs (snd pointerBoardBefore - snd pointerBoardAfter) < 0.000_001)
        "Pointer-centered editor zoom, camera framing, or deterministic camera evidence changed."

    let resized =
        MapEditorWorkspace.initial false
        |> MapEditorWorkspace.update
            editor.Map
            (MapEditor.selected editor)
            (ResizeViewport(720.0, 480.0))
    let mousePointer =
        { Id = 7
          Kind = MousePointer
          X = 100.0
          Y = 90.0
          RequestsPan = true }
    let captured =
        resized
        |> MapEditorWorkspace.update
            editor.Map
            (MapEditor.selected editor)
            (StartEditorPointer mousePointer)
    let mousePanned =
        captured
        |> MapEditorWorkspace.update
            editor.Map
            (MapEditor.selected editor)
            (MoveEditorPointer { mousePointer with X = 130.0; Y = 110.0 })
    let released =
        mousePanned
        |> MapEditorWorkspace.update
            editor.Map
            (MapEditor.selected editor)
            (LoseEditorPointerCapture mousePointer.Id)
    require
        (resized.ViewportWidth = 720.0
         && resized.ViewportHeight = 480.0
         && Map.containsKey mousePointer.Id captured.CapturedPointers
         && mousePanned.Camera.PanX = captured.Camera.PanX + 30.0
         && mousePanned.Camera.PanY = captured.Camera.PanY + 20.0
         && released.CapturedPointers.IsEmpty)
        "Editor resize, pointer capture, drag pan, or lost-capture cleanup failed."

    let firstTouch =
        { Id = 10
          Kind = TouchPointer
          X = 100.0
          Y = 100.0
          RequestsPan = true }
    let secondTouch =
        { Id = 11
          Kind = TouchPointer
          X = 200.0
          Y = 100.0
          RequestsPan = true }
    let touched =
        MapEditorWorkspace.initial false
        |> MapEditorWorkspace.update
            editor.Map
            None
            (StartEditorPointer firstTouch)
        |> MapEditorWorkspace.update
            editor.Map
            None
            (StartEditorPointer secondTouch)
    let pinched =
        touched
        |> MapEditorWorkspace.update
            editor.Map
            None
            (MoveEditorPointer { secondTouch with X = 220.0 })
        |> MapEditorWorkspace.update
            editor.Map
            None
            (EndEditorPointer firstTouch.Id)
        |> MapEditorWorkspace.update
            editor.Map
            None
            (EndEditorPointer secondTouch.Id)
        |> MapEditorWorkspace.update
            editor.Map
            None
            (SetEditorReducedMotion true)
    require
        (pinched.Camera.Zoom > touched.Camera.Zoom
         && pinched.CapturedPointers.IsEmpty
         && pinched.ReducedMotion)
        "Two-pointer touch camera behavior, release cleanup, or reduced-motion state failed."

    let initialRevisionDigest = editor.Revision.Digest
    let historyFixture =
        [ "initial-digest=" + initialRevisionDigest
          "history-command-limit=" + string MapEditor.MaximumHistoryCommands
          "history-byte-limit=" + string MapEditor.MaximumHistoryBytes
          "selection-order="
          + (editor.Map.Units |> Map.toList |> List.map (fst >> string) |> String.concat ",")
          "revision-states="
          + ([ SavedRevision; DirtyRevision; SimulatedRevision; RecoveredRevision ]
             |> List.map string
             |> String.concat ",") ]
        |> String.concat "\n"
    let expectedHistoryFixture =
        Path.Combine(
            AppContext.BaseDirectory,
            "fixtures",
            "map-editor-milestone-2-history.txt"
        )
        |> File.ReadAllText
        |> fun value -> value.TrimEnd('\r', '\n')
    require
        (historyFixture = expectedHistoryFixture
         && initialRevisionDigest = MapEditor.revisionDigest editor.Map
         && initialRevisionDigest.Length = 64)
        "The stable map revision digest or deterministic Milestone 2 fixture changed."

    let additiveSelection =
        editor
        |> MapEditor.update (SelectEditorUnit(Some 1))
        |> MapEditor.update (ToggleEditorUnitSelection 3)
    let boxSelection =
        editor
        |> MapEditor.update (
            BeginEditorBoxSelection
                { CellColumn = 0
                  CellRow = 0 }
        )
        |> MapEditor.update (
            ExtendEditorBoxSelection
                { CellColumn = 4
                  CellRow = 7 }
        )
        |> MapEditor.update CommitEditorGesture
    require
        (additiveSelection.SelectedUnits = Set.ofList [ 1; 3 ]
         && boxSelection.SelectedUnits = Set.ofList [ 1; 2 ]
         && boxSelection.Gesture = IdleGesture
         && (editor |> MapEditor.update SelectAllInActiveDomain).SelectedUnits.Count
            = editor.Map.Units.Count)
        "Click, additive, box, or active-domain select-all selection failed."

    let duplicated =
        editor
        |> MapEditor.update (SelectEditorUnit(Some 3))
        |> MapEditor.update CopyEditorSelection
        |> MapEditor.update PasteEditorClipboard
    let duplicateDigest = duplicated.Revision.Digest
    let duplicateUndone = duplicated |> MapEditor.update UndoEditorCommand
    let duplicateRedone = duplicateUndone |> MapEditor.update RedoEditorCommand
    let deleted =
        duplicated
        |> MapEditor.update DeleteEditorSelection
    let deleteUndone = deleted |> MapEditor.update UndoEditorCommand
    require
        (duplicated.Map.Units.Count = editor.Map.Units.Count + 1
         && duplicated.SelectedUnits.Count = 1
         && duplicateUndone.Map = editor.Map
         && duplicateRedone.Map = duplicated.Map
         && duplicateRedone.Revision.Digest = duplicateDigest
         && deleted.Map.Units.Count = editor.Map.Units.Count
         && deleteUndone.Map = duplicated.Map)
        "Copy, paste, delete, undo, or redo did not preserve an immutable unit revision."

    for index in 0 .. 63 do
        let column = int32 (index % int editor.Map.Width)
        let row = int32 ((index * 5) % int editor.Map.Height)
        let current = MapEditor.terrainAt column row editor
        let replacement = if current = Rough then Objective else Rough
        let command =
            PaintCells(
                replacement,
                [| { CellColumn = column
                     CellRow = row } |]
            )
        let committed =
            { editor with Gesture = CommandPreviewGesture command }
            |> MapEditor.update CommitEditorGesture
        let undone = committed |> MapEditor.update UndoEditorCommand
        let redone = undone |> MapEditor.update RedoEditorCommand
        require
            (committed.Map <> editor.Map
             && undone.Map = editor.Map
             && undone.Revision.Digest = initialRevisionDigest
             && redone.Map = committed.Map
             && redone.Revision.Digest = committed.Revision.Digest)
            ("Property round-trip failed for generated command " + string index + ".")

    let boundedHistory =
        [ 0 .. 139 ]
        |> List.fold (fun state index ->
            let terrain = if index % 2 = 0 then Rough else Open
            { state with
                Gesture =
                    CommandPreviewGesture(
                        PaintCells(
                            terrain,
                            [| { CellColumn = 0
                                 CellRow = 0 } |]
                        )
                    ) }
            |> MapEditor.update CommitEditorGesture) editor
    require
        (boundedHistory.UndoHistory.Length = MapEditor.MaximumHistoryCommands
         && boundedHistory.HistoryBytes <= MapEditor.MaximumHistoryBytes
         && boundedHistory.RedoHistory.IsEmpty)
        "Editor history exceeded its command-count or serialized-size bound."

    let revisionStates =
        duplicated
        |> MapEditor.update MarkEditorSaved
        |> fun saved ->
            require
                (saved.RevisionState = SavedRevision
                 && saved.SavedDigest = Some saved.Revision.Digest)
                "Saving did not mark the exact immutable revision."
            saved
        |> MapEditor.update MarkEditorSimulated
        |> fun simulated ->
            require
                (simulated.RevisionState = SimulatedRevision
                 && simulated.SimulatedDigest = Some simulated.Revision.Digest)
                "Simulation did not snapshot the exact immutable revision."
            simulated
        |> MapEditor.update (MarkEditorRecovered initialRevisionDigest)
    require
        (revisionStates.RevisionState = RecoveredRevision
         && revisionStates.RecoveredFromDigest = Some initialRevisionDigest)
        "Recovered revision provenance was not retained."
    let simulatedRuntime =
        editor
        |> MapEditor.update MarkEditorSimulated
        |> MapEditor.update StepEditor
        |> MapEditor.update (SetSelectedHealth(6, 12))
    let restoredDraft = simulatedRuntime |> MapEditor.update RestoreEditorDraft
    require
        (simulatedRuntime.Revision.Digest = initialRevisionDigest
         && simulatedRuntime.UndoHistory.IsEmpty
         && simulatedRuntime.Map <> simulatedRuntime.Revision.Document
         && restoredDraft.Map = editor.Map
         && restoredDraft.Revision.Digest = initialRevisionDigest)
        "Simulator runtime state entered authored history or changed revision identity."

    let address column row =
        { CellColumn = int32 column
          CellRow = int32 row }
    let previewCells state =
        MapEditor.terrainPreview state
        |> Option.map (fun (_, addresses, isValid) -> addresses, isValid)
        |> Option.defaultWith (fun () -> failwith "Expected a terrain preview.")
    let terrainPreview tool terrain brush first last =
        editor
        |> MapEditor.update (ChooseTerrain terrain)
        |> MapEditor.update (SetTerrainBrushSize(int32 brush))
        |> MapEditor.update (ChooseTool(Terrain tool))
        |> MapEditor.update (BeginTerrainGesture first)
        |> MapEditor.update (ExtendTerrainGesture last)

    let pencilPreview =
        editor
        |> MapEditor.update (ChooseTerrain Rough)
        |> MapEditor.update (SetTerrainBrushSize 1)
        |> MapEditor.update (ChooseTool(Terrain PencilTool))
        |> MapEditor.update (BeginTerrainGesture(address 0 7))
        |> MapEditor.update (ExtendTerrainGesture(address 2 7))
        |> MapEditor.update (ExtendTerrainGesture(address 2 5))
    let pencilCells, pencilValid = previewCells pencilPreview
    let pencilCommitted = pencilPreview |> MapEditor.update CommitEditorGesture
    let pencilUndone = pencilCommitted |> MapEditor.update UndoEditorCommand
    let linePreview =
        terrainPreview LineTool Objective 1 (address 7 0) (address 11 4)
    let lineCells, lineValid = previewCells linePreview
    let rectanglePreview =
        terrainPreview RectangleTool Rough 1 (address 7 0) (address 9 2)
    let rectangleCells, rectangleValid = previewCells rectanglePreview
    let boundaryBrushPreview =
        terrainPreview PencilTool Rough 3 (address 0 0) (address 0 0)
    let boundaryBrushCells, boundaryBrushValid = previewCells boundaryBrushPreview
    let floodPreview =
        terrainPreview FloodFillTool Objective 1 (address 0 0) (address 0 0)
    let floodCells, floodValid = previewCells floodPreview
    let sampled =
        editor
        |> MapEditor.update (ChooseTool(Terrain EyedropperTool))
        |> MapEditor.update (BeginTerrainGesture(address 5 3))
    let erased =
        editor
        |> MapEditor.update (ChooseTool(Terrain EraseTool))
        |> MapEditor.update (BeginTerrainGesture(address 4 3))
        |> MapEditor.update CommitEditorGesture
    let blockedPreview =
        terrainPreview RectangleTool Blocked 1 (address 1 1) (address 2 2)
    let _, blockedValid = previewCells blockedPreview
    let blockedRejected = blockedPreview |> MapEditor.update CommitEditorGesture
    let fallbackPainted =
        editor
        |> MapEditor.update (ChooseTerrain Rough)
        |> MapEditor.update (ChooseTool(Terrain PencilTool))
        |> MapEditor.update (ActivateCell(0, 0))

    require
        (pencilValid
         && pencilCells = [| address 2 5; address 2 6; address 0 7; address 1 7; address 2 7 |]
         && pencilCommitted.UndoHistory.Length = editor.UndoHistory.Length + 1
         && pencilCommitted.Revision.Number = editor.Revision.Number + 1L
         && pencilUndone.Map = editor.Map
         && lineValid
         && lineCells = [| address 7 0; address 8 1; address 9 2; address 10 3; address 11 4 |]
         && rectangleValid
         && rectangleCells.Length = 9
         && boundaryBrushValid
         && boundaryBrushCells = [| address 0 0; address 1 0; address 0 1; address 1 1 |]
         && floodValid
         && floodCells.Length = 90
         && sampled.TerrainSelection = Objective
         && MapEditor.terrainAt 4 3 erased = Open
         && not blockedValid
         && blockedRejected.Map = editor.Map
         && blockedRejected.Revision = editor.Revision
         && blockedRejected.UndoHistory = editor.UndoHistory
         && blockedRejected.Validation.IsSome
         && MapEditor.terrainAt 0 0 fallbackPainted = Rough
         && fallbackPainted.Revision.Number = editor.Revision.Number + 1L)
        "Terrain tools, deterministic previews, atomic history, eyedropper, erase, or occupied-footprint rejection failed."

    let terrainFixture =
        [ "tools="
          + ([ PencilTool; RectangleTool; LineTool; FloodFillTool; EyedropperTool; EraseTool ]
             |> List.map MapEditor.terrainToolLabel
             |> String.concat ",")
          "shortcuts="
          + ([ PencilTool; RectangleTool; LineTool; FloodFillTool; EyedropperTool; EraseTool ]
             |> List.map MapEditor.terrainToolShortcut
             |> String.concat ",")
          "patterns="
          + ([ Open; Rough; Blocked; Objective ]
             |> List.map MapEditor.terrainPattern
             |> String.concat ",")
          "pencil="
          + (pencilCells
             |> Array.map (fun cell -> string cell.CellColumn + "," + string cell.CellRow)
             |> String.concat ";")
          "line="
          + (lineCells
             |> Array.map (fun cell -> string cell.CellColumn + "," + string cell.CellRow)
             |> String.concat ";")
          "rectangle-count=" + string rectangleCells.Length
          "boundary-brush="
          + (boundaryBrushCells
             |> Array.map (fun cell -> string cell.CellColumn + "," + string cell.CellRow)
             |> String.concat ";")
          "maximum-map=40x40" ]
        |> String.concat "\n"
    let expectedTerrainFixture =
        Path.Combine(
            AppContext.BaseDirectory,
            "fixtures",
            "map-editor-milestone-3-terrain.txt"
        )
        |> File.ReadAllText
        |> fun value -> value.TrimEnd('\r', '\n')
    require
        (terrainFixture = expectedTerrainFixture)
        "The deterministic Milestone 3 terrain review fixture changed."

    let unitPresetFixture =
        MapEditor.searchCanonicalUnitPresets ""
        |> List.map (fun preset ->
            String.concat
                "|"
                [ preset.Faction
                  preset.Role
                  preset.Id
                  preset.ClassId
                  preset.GlyphId
                  string preset.Side
                  string preset.FootprintSize
                  string preset.Health
                  string preset.HealthMaximum ])
        |> String.concat "\n"
    let expectedUnitPresetFixture =
        Path.Combine(
            AppContext.BaseDirectory,
            "fixtures",
            "map-editor-milestone-4-units.txt"
        )
        |> File.ReadAllText
        |> fun value -> value.TrimEnd('\r', '\n')
    require
        (unitPresetFixture = expectedUnitPresetFixture
         && (MapEditor.searchCanonicalUnitPresets "heavy" |> List.map _.Id) = [ "troll" ]
         && (MapEditor.searchCanonicalUnitPresets "human" |> List.map _.Id) = [ "human" ])
        "Canonical unit search, grouping, or explicit defaults changed."

    let unit id size column row =
        { Id = id
          Side = Blue
          ClassId = "rifleman"
          Column = column
          Row = row
          Size = size
          Health = 12
          HealthMaximum = 12
          Controller = Manual
          Script = []
          ScriptIndex = 0 }
    let unitTestMap size =
        { editor.Map with
            Width = 8
            Height = 8
            Terrain = Map.empty
            Edges = Map.empty
            Units = Map.empty
            NextUnitId = 1 }
        |> fun map ->
            let candidate = unit 1 size (8 - size) (8 - size)
            map, candidate
    for size in [ 1; 2; 3; 8 ] do
        let map, candidate = unitTestMap size
        require
            (MapEditor.validateCommand map (AddUnits [| candidate |]) |> Result.isOk)
            (string size + "x" + string size + " footprint did not fit the exact border.")
        require
            (MapEditor.validateCommand map (AddUnits [| { candidate with Column = 9 - size } |])
             |> Result.isError)
            (string size + "x" + string size + " footprint crossed the map border.")
        let blocked =
            { map with Terrain = Map.ofList [ (7, 7), Blocked ] }
        require
            (MapEditor.validateCommand blocked (AddUnits [| candidate |]) |> Result.isError)
            (string size + "x" + string size + " footprint ignored blocked terrain.")
        let occupied =
            { map with
                Units = Map.ofList [ 9, unit 9 1 candidate.Column candidate.Row ]
                NextUnitId = 10 }
        require
            (MapEditor.validateCommand occupied (AddUnits [| { candidate with Id = 10 } |])
             |> Result.isError)
            (string size + "x" + string size + " footprint overlapped another unit.")
        let movingUnit = unit 1 size 0 0
        let edgeMap =
            { map with
                Edges = Map.ofList [ (size - 1, 0, EastEdge), (Wall, false) ]
                Units = Map.ofList [ 1, movingUnit ]
                NextUnitId = 2 }
        let edgeRevision =
            { editor.Revision with
                Document = edgeMap
                Digest = MapEditor.revisionDigest edgeMap }
        let edgeState =
            { editor with
                Map = edgeMap
                Revision = edgeRevision
                SelectedUnit = Some 1
                SelectedUnits = Set.singleton 1 }
        let edgeRejected = MapEditor.update (MoveSelected East) edgeState
        require
            (edgeRejected.Map = edgeMap
             && edgeRejected.Revision.Digest = edgeRevision.Digest)
            (string size + "x" + string size + " footprint crossed a blocking edge.")

    let formationMap =
        { editor.Map with
            Width = 8
            Height = 8
            Terrain = Map.empty
            Edges = Map.ofList [ (1, 0, EastEdge), (Wall, false) ]
            Units = Map.ofList [ 1, unit 1 1 1 0; 2, unit 2 2 1 2 ]
            NextUnitId = 3 }
    let formationState =
        { editor with
            Map = formationMap
            Revision =
                { editor.Revision with
                    Document = formationMap
                    Digest = MapEditor.revisionDigest formationMap }
            SelectedUnit = Some 1
            SelectedUnits = Set.ofList [ 1; 2 ] }
    let blockedFormation = MapEditor.update (MoveSelected East) formationState
    require
        (blockedFormation.Map = formationMap
         && blockedFormation.Revision.Digest = formationState.Revision.Digest)
        "A multiselection crossed a blocking leading edge or partially committed."
    let movableFormation = { formationState with Map = { formationMap with Edges = Map.empty }; Revision = { formationState.Revision with Document = { formationMap with Edges = Map.empty }; Digest = MapEditor.revisionDigest { formationMap with Edges = Map.empty } } }
    let movedFormation = MapEditor.update (MoveSelected East) movableFormation
    let duplicatedFormation =
        movedFormation
        |> MapEditor.update CopyEditorSelection
        |> MapEditor.update PasteEditorClipboard
    require
        ((Map.find 1 movedFormation.Map.Units).Column = 2
         && (Map.find 2 movedFormation.Map.Units).Column = 2
         && movedFormation.Revision.Number = movableFormation.Revision.Number + 1L
         && duplicatedFormation.Map.Units.Count = 4
         && duplicatedFormation.SelectedUnits = Set.ofList [ 3; 4 ]
         && duplicatedFormation.Revision.Number = movedFormation.Revision.Number + 1L)
        "Formation movement or copy/paste was not one validated atomic revision."

    let placementPreview =
        { formationState with Map = { formationMap with Edges = Map.empty; Units = Map.empty; NextUnitId = 1 } }
        |> MapEditor.update (ChooseTool(Place(Red, "troll", 3)))
        |> MapEditor.update (PreviewUnitPlacement(address 5 5))
    require
        (match MapEditor.unitPreview placementPreview with
         | Some(units, true) ->
             units.Length = 1
             && units[0].HealthMaximum = 240
             && units[0].Size = 3
         | _ -> false)
        "Placement preview omitted the complete footprint or canonical defaults."

    let edgeBase = MapEditor.update ClearMap editor
    let wallPreview =
        edgeBase
        |> MapEditor.update (ChooseTool(Edge(EastEdge, Wall)))
        |> MapEditor.update (ActivateEdge(1, 1, EastEdge))
        |> MapEditor.update (ActivateEdge(1, 2, EastEdge))
    let duplicatePreview =
        wallPreview |> MapEditor.update (ActivateEdge(1, 2, EastEdge))
    require
        (duplicatePreview.Validation = Some "This canonical edge is already in the polyline."
         && duplicatePreview.Revision.Digest = edgeBase.Revision.Digest)
        "A duplicate polyline segment was not linted before commit."
    let connectedPreview =
        edgeBase
        |> MapEditor.update (ChooseTool(Edge(EastEdge, Wall)))
        |> MapEditor.update (ActivateEdge(1, 1, EastEdge))
        |> MapEditor.update (ActivateEdge(4, 3, SouthEdge))
    require
        (match connectedPreview.Gesture with
         | EdgePolylineGesture(Wall, segments) ->
             segments.Length > 2 && segments.Length = (segments |> Array.distinct |> Array.length)
         | _ -> false)
        "Separated snapped clicks did not resolve to one continuous canonical polyline."
    let emptyAfterEscape =
        edgeBase
        |> MapEditor.update (ChooseTool(Edge(EastEdge, Wall)))
        |> MapEditor.update (ActivateEdge(1, 1, EastEdge))
        |> MapEditor.update CancelEditorGesture
    let canceledAfterEscape =
        emptyAfterEscape |> MapEditor.update CancelEditorGesture
    require
        ((match emptyAfterEscape.Gesture with
          | EdgePolylineGesture(Wall, segments) -> Array.isEmpty segments
          | _ -> false)
         && canceledAfterEscape.Gesture = IdleGesture)
        "Escape did not remove the last segment before canceling an empty polyline."
    let committedOnSwitch =
        edgeBase
        |> MapEditor.update (ChooseTool(Edge(EastEdge, Wall)))
        |> MapEditor.update (ActivateEdge(2, 2, EastEdge))
        |> MapEditor.update (ChooseTool(Terrain PencilTool))
    require
        (Map.containsKey (2, 2, EastEdge) committedOnSwitch.Map.Edges
         && committedOnSwitch.Revision.Number = edgeBase.Revision.Number + 1L)
        "Switching tools did not finish the active edge polyline."
    let backed = duplicatePreview |> MapEditor.update BacktrackEdgePolyline
    let wallCommitted =
        backed
        |> MapEditor.update (ActivateEdge(1, 2, EastEdge))
        |> MapEditor.update FinishEdgePolyline
    let wallKeys =
        wallCommitted.Map.Edges
        |> Map.toList
        |> List.map fst
    require
        (wallKeys = [ (1, 1, EastEdge); (1, 2, EastEdge) ]
         && wallCommitted.Revision.Number = edgeBase.Revision.Number + 1L)
        "A wall polyline did not commit once through revision history."

    let editedEdges =
        wallCommitted
        |> MapEditor.update (ConvertEdge(1, 1, EastEdge, Door))
        |> MapEditor.update (ToggleDoorState(1, 1, EastEdge))
        |> MapEditor.update (ConvertEdge(1, 2, EastEdge, Window))
        |> MapEditor.update (JoinEdge(2, 2, SouthEdge))
        |> MapEditor.update (SplitEdge(2, 2, SouthEdge))
    require
        (Map.tryFind (1, 1, EastEdge) editedEdges.Map.Edges = Some(Door, true)
         && Map.tryFind (1, 2, EastEdge) editedEdges.Map.Edges = Some(Window, false)
         && not (Map.containsKey (2, 2, SouthEdge) editedEdges.Map.Edges))
        "Door/window conversion, state editing, split, or join changed edge meaning."
    let erasedAndRestored =
        editedEdges
        |> MapEditor.update (EraseEdge(1, 2, EastEdge))
        |> MapEditor.update UndoEditorCommand
        |> MapEditor.update RedoEditorCommand
    require
        (not (Map.containsKey (1, 2, EastEdge) erasedAndRestored.Map.Edges))
        "Edge erase did not participate in undo/redo revision history."

    require
        (MapEditor.tryNormalizeEdge 12 8 2 1 2 2 = Some(1, 1, EastEdge)
         && MapEditor.tryNormalizeEdge 12 8 2 2 1 2 = Some(1, 1, SouthEdge)
         && MapEditor.tryNormalizeEdge 12 8 0 0 0 1 = None
         && MapEditor.tryNormalizeEdge 12 8 0 0 1 0 = None)
        "Physical edge gestures did not normalize exactly once or reject ownerless borders."

    let gapMap =
        { edgeBase.Map with
            Edges =
                [ (3, 1, EastEdge), (Wall, false)
                  (3, 3, EastEdge), (Wall, false) ]
                |> Map.ofList }
    require
        (MapEditor.edgeIssues gapMap |> List.map _.Code = [ "EDGE-GAP" ])
        "Collinear semantic edge gaps were not linted deterministically."
    let borderCodes =
        MapEditor.edgeIssues
            { edgeBase.Map with
                Edges = Map.ofList [ (-1, 0, EastEdge), (Wall, false) ] }
        |> List.map _.Code
    require
        (borderCodes = [ "EDGE-BORDER" ])
        "A canonical edge without an owning border cell was not linted."
    let overlapCodes =
        MapEditor.validateCommand
            edgeBase.Map
            (ReplaceEdges
                [| (1, 1, EastEdge), Some(Wall, false)
                   (1, 1, EastEdge), Some(Door, false) |])
        |> function
            | Error issues -> issues |> List.map _.Code
            | Ok _ -> []
    let duplicateCodes =
        MapEditor.validateCommand
            edgeBase.Map
            (ReplaceEdges
                [| (1, 1, EastEdge), Some(Wall, false)
                   (1, 1, EastEdge), Some(Wall, false) |])
        |> function
            | Error issues -> issues |> List.map _.Code
            | Ok _ -> []
    require
        (overlapCodes = [ "EDGE-OVERLAP" ]
         && duplicateCodes = [ "EDGE-DUPLICATE" ])
        "Duplicate or overlapping edge replacements passed command validation."
    require
        (MapEditor.tryImport
             "SIR-MAP 1\nsize 4 4\nedge 1 1 east wall closed\nedge 1 1 east door closed\n"
         |> Result.isError)
        "Duplicate canonical edge records in an imported map were silently overwritten."
    require
        (MapEditor.tryImport
             "SIR-MAP 1\nsize 4 4\nedge 1 1 east window open\n"
         |> Result.isError)
        "Open state on a non-door edge passed import validation."

    let unitOne = Map.find 1 editor.Map.Units
    let blockedLeadingSide =
        { editor.Map with
            Edges =
                [ (unitOne.Column + unitOne.Size - 1, unitOne.Row, EastEdge),
                    (Wall, false) ]
                |> Map.ofList }
    let leadingCodes =
        MapEditor.leadingSideMovementIssues blockedLeadingSide East [| unitOne |]
        |> List.map _.Code
    require
        (leadingCodes = [ "EDGE-LEADING-SIDE" ])
        "Movement lint did not inspect the complete leading side."

    let edgeRoundTripText = MapEditor.export editedEdges
    let edgeRoundTrip =
        MapEditor.tryImport edgeRoundTripText
        |> Result.defaultWith failwith
    let edgeRoundTripState = { editedEdges with Map = edgeRoundTrip }
    require
        (edgeRoundTrip.Edges = editedEdges.Map.Edges
         && MapEditor.export edgeRoundTripState = edgeRoundTripText)
        "SIR-MAP round-trip changed edge meaning or canonical record order."

    let edgeFixture =
        String.concat
            "\n"
            [ "normalize-east="
              + string (MapEditor.tryNormalizeEdge 12 8 2 1 2 2)
              "normalize-south="
              + string (MapEditor.tryNormalizeEdge 12 8 2 2 1 2)
              "polyline=" + string (wallCommitted.Map.Edges |> Map.toList)
              "edited=" + string (editedEdges.Map.Edges |> Map.toList)
              "gap-codes="
              + (MapEditor.edgeIssues gapMap
                 |> List.map _.Code
                 |> String.concat ",")
              "border-codes=" + String.concat "," borderCodes
              "duplicate-codes=" + String.concat "," duplicateCodes
              "overlap-codes=" + String.concat "," overlapCodes
              "leading-codes="
              + (MapEditor.leadingSideMovementIssues blockedLeadingSide East [| unitOne |]
                 |> List.map _.Code
                 |> String.concat ",")
              "round-trip=" + string (MapEditor.export edgeRoundTripState = edgeRoundTripText) ]
    let expectedEdgeFixture =
        File.ReadAllText(
            Path.Combine(
                __SOURCE_DIRECTORY__,
                "fixtures",
                "map-editor-milestone-5-edges.txt"
            )
        ).TrimEnd()
    require
        (edgeFixture = expectedEdgeFixture)
        ("The deterministic Milestone 5 semantic-edge review fixture changed.\n"
         + edgeFixture)

    let hiddenEdgeState =
        editor
        |> MapEditor.update (SetEditorLayerState(EdgeDomain, HiddenLayer))
    let gapIssues =
        MapEditor.validationIssues gapMap
    let issueState =
        { hiddenEdgeState with
            Map = gapMap
            Issues = gapIssues
            ActiveIssue = Some 0 }
        |> MapEditor.update SelectNextIssue
        |> MapEditor.update SelectPreviousIssue
    require
        (MapEditor.layerState EdgeDomain hiddenEdgeState = HiddenLayer
         && gapIssues |> Array.map _.Code = [| "EDGE-GAP" |]
         && issueState.ActiveIssue = Some 0)
        "Hidden edge content stopped validating or issue navigation was unstable."

    let lockedTerrain =
        editor
        |> MapEditor.update (ChooseTool(Paint Rough))
        |> MapEditor.update (SetEditorLayerState(TerrainDomain, LockedLayer))
    let lockedAttempt = MapEditor.update (ActivateCell(0, 0)) lockedTerrain
    require
        (lockedAttempt.Map = lockedTerrain.Map
         && lockedAttempt.Validation |> Option.exists (_.Contains("locked")))
        "A locked terrain layer accepted an edit."

    let resizeRequested = MapEditor.update (Resize(4, 4)) editor
    let resizeLoss =
        match resizeRequested.PendingDestructiveChange with
        | Some(ResizePending loss) -> loss
        | _ -> failwith "Resize did not require an explicit confirmation."
    let resizeCanceled =
        resizeRequested |> MapEditor.update CancelDestructiveChange
    let resizeConfirmed =
        resizeRequested |> MapEditor.update ConfirmDestructiveChange
    require
        (resizeLoss.LostTerrainCells = 6
         && resizeLoss.LostEdges = 3
         && resizeLoss.LostUnits = 3
         && resizeCanceled.Map = editor.Map
         && resizeConfirmed.Map.Width = 4
         && resizeConfirmed.Map.Height = 4
         && resizeConfirmed.Map.Units.Count = 1
         && resizeConfirmed.Revision.Number = editor.Revision.Number + 1L)
        "Safe resize did not preview loss, cancel cleanly, or commit atomically."

    let invalidImport = MapEditor.update (LoadMapText "not a map") editor
    require
        (invalidImport.Map = editor.Map
         && invalidImport.Revision.Digest = editor.Revision.Digest)
        "A failed import partially replaced canonical map state."

    let recoveryText = MapEditor.export (MapEditor.update ClearMap editor)
    let recoveryOffered =
        editor |> MapEditor.update (OfferCrashRecovery recoveryText)
    let recoveryDiscarded =
        recoveryOffered |> MapEditor.update DiscardCrashDraft
    let recoveryAccepted =
        recoveryOffered |> MapEditor.update RecoverCrashDraft
    require
        (recoveryOffered.PendingRecovery.IsSome
         && recoveryDiscarded.Map = editor.Map
         && recoveryAccepted.Map.Units.IsEmpty
         && recoveryAccepted.RevisionState = RecoveredRevision)
        "Crash recovery did not require an explicit recover/discard choice."

    let metadataState =
        editor
        |> MapEditor.update (SetMapName "Bridge at dusk")
        |> MapEditor.update (
            SaveMapView(
                "Overview",
                { PanX = 14.0
                  PanY = 28.0
                  Zoom = 1.5 }
            )
        )
    let thumbnail = MapEditor.thumbnailSvg metadataState
    let metadataWithThumbnail =
        metadataState |> MapEditor.update (SetMapThumbnail(Some thumbnail))
    require
        (metadataWithThumbnail.Revision.Digest = editor.Revision.Digest
         && MapEditor.export metadataWithThumbnail = MapEditor.export editor
         && metadataWithThumbnail.Authoring.SavedViews.Count = 1
         && metadataWithThumbnail.Authoring.ThumbnailSvg = Some thumbnail
         && thumbnail = MapEditor.thumbnailSvg metadataState)
        "Authoring metadata changed canonical identity or thumbnail generation was unstable."

    let hiddenSimulation =
        editor
        |> MapEditor.update (SetEditorLayerState(UnitDomain, HiddenLayer))
        |> MapEditor.update StepEditor
    require
        (hiddenSimulation.Tick = 1
         && hiddenSimulation.LastEvents.Length = editor.Map.Units.Count)
        "Hidden units stopped participating in simulation."

    let clearRequested =
        editor |> MapEditor.update RequestClearMap
    let clearConfirmed =
        clearRequested |> MapEditor.update ConfirmDestructiveChange
    require
        (clearRequested.Map = editor.Map
         && clearRequested.PendingDestructiveChange = Some ClearPending
         && clearConfirmed.Map.Units.IsEmpty)
        "Clear did not require confirmation or commit as one revision."

    let lifecycleFixture =
        String.concat
            "\n"
            [ "layers="
              + ([ TerrainDomain; EdgeDomain; UnitDomain; RegionDomain; DocumentDomain ]
                 |> List.map (fun domain ->
                     string domain + ":" + string (MapEditor.layerState domain hiddenEdgeState))
                 |> String.concat ",")
              "hidden-validation=" + (gapIssues |> Array.map _.Code |> String.concat ",")
              "issue-index=" + string issueState.ActiveIssue
              "locked-edit-preserved=" + string (lockedAttempt.Map = lockedTerrain.Map)
              "resize-loss="
              + String.concat
                    ","
                    [ string resizeLoss.LostTerrainCells
                      string resizeLoss.LostEdges
                      string resizeLoss.LostUnits ]
              "resize-confirmed="
              + string resizeConfirmed.Map.Width + "x" + string resizeConfirmed.Map.Height
              + ":" + string resizeConfirmed.Map.Units.Count
              "atomic-import=" + string (invalidImport.Map = editor.Map)
              "recovery-choice="
              + string recoveryOffered.PendingRecovery.IsSome + ":"
              + string (recoveryDiscarded.Map = editor.Map) + ":"
              + string recoveryAccepted.Map.Units.Count
              "metadata="
              + metadataWithThumbnail.Authoring.Name + ":"
              + string metadataWithThumbnail.Authoring.SavedViews.Count + ":"
              + string (metadataWithThumbnail.Revision.Digest = editor.Revision.Digest)
              "thumbnail-deterministic=" + string (thumbnail = MapEditor.thumbnailSvg metadataState)
              "hidden-simulation=" + string hiddenSimulation.Tick + ":" + string hiddenSimulation.LastEvents.Length
              "clear-confirmed=" + string clearConfirmed.Map.Units.Count ]
    let expectedLifecycleFixture =
        File.ReadAllText(
            Path.Combine(
                __SOURCE_DIRECTORY__,
                "fixtures",
                "map-editor-milestone-6-lifecycle.txt"
            )
        ).TrimEnd()
    require
        (lifecycleFixture = expectedLifecycleFixture)
        ("The deterministic Milestone 6 lifecycle review fixture changed.\n"
         + lifecycleFixture)

    let zoneAuthored =
        editor
        |> MapEditor.update (
            CreateRectangleRegion(
                ObjectiveRegion,
                address 2 2,
                address 4 3
            )
        )
        |> MapEditor.update (
            CreatePolygonRegion(
                DeploymentZone Red,
                [| address 6 1; address 9 1; address 8 4 |]
            )
        )
        |> MapEditor.update (SelectEditorRegion(Some 1))
        |> MapEditor.update (MoveSelectedRegion(1, 0))
        |> MapEditor.update (SetSelectedRegionPurpose(DeploymentZone Blue))
        |> MapEditor.update (SelectEditorRegion(Some 2))
        |> MapEditor.update (MoveSelectedRegionVertex(2, -1, 0))
    let zoneText = MapEditor.export zoneAuthored
    let zoneImported =
        MapEditor.tryImport zoneText |> Result.defaultWith failwith
    let legacyV1 =
        "SIR-MAP 1\nsize 4 4\nterrain 1 1 rough\n"
    let migratedV1 =
        MapEditor.tryImport legacyV1 |> Result.defaultWith failwith
    let migratedText =
        MapEditor.export { editor with Map = migratedV1 }
    let invalidBowTie =
        { Id = 99
          Purpose = ObjectiveRegion
          Geometry =
            RegionPolygon(
                [| address 1 1
                   address 4 4
                   address 1 4
                   address 4 1 |]
            )
          Behavior = NoRegionBehavior }
    let invalidRegionCodes =
        match MapEditor.validateCommand zoneAuthored.Map (AddRegions [| invalidBowTie |]) with
        | Ok _ -> []
        | Error issues -> issues |> List.map _.Code
    let lockedZone =
        zoneAuthored
        |> MapEditor.update (SetEditorLayerState(RegionDomain, LockedLayer))
    let lockedZoneAttempt =
        lockedZone |> MapEditor.update (MoveSelectedRegion(1, 0))
    let zoneUndone = zoneAuthored |> MapEditor.update UndoEditorCommand
    let zoneRedone = zoneUndone |> MapEditor.update RedoEditorCommand
    let zoneResizeLoss = MapEditor.resizeLossPreview 4 4 zoneAuthored.Map
    let hiddenInvalidRegions =
        { zoneAuthored.Map with
            Regions = Map.add invalidBowTie.Id invalidBowTie zoneAuthored.Map.Regions }
        |> MapEditor.validationIssues
        |> Array.map _.Code
        |> Array.filter (_.StartsWith("REGION-"))
    let zoneFixture =
        String.concat
            "\n"
            [ zoneText.TrimEnd()
              "round-trip=" + string (zoneImported = zoneAuthored.Map)
              "v1-load=" + string (migratedV1.Regions.IsEmpty)
              "v1-canonical-header=" + migratedText.Split('\n')[0]
              "invalid-codes=" + String.concat "," invalidRegionCodes
              "locked-preserved=" + string (lockedZoneAttempt.Map = lockedZone.Map)
              "undo-redo=" + string (zoneRedone.Map = zoneAuthored.Map)
              "resize-region-loss=" + string zoneResizeLoss.LostRegions
              "hidden-validation=" + String.concat "," hiddenInvalidRegions
              "invalid-import-rejected="
              + string (
                  MapEditor.tryImport (
                      "SIR-MAP 2\nsize 6 6\nzone 1 objective polygon 1,1 4,4 1,4 4,1\n"
                  )
                  |> Result.isError
              )
              "macro-rejected="
              + string (
                  MapEditor.tryImport (
                      "SIR-MAP 2\nsize 4 4\nmacro 1 trusted launch\n"
                  )
                  |> Result.isError
              )
              "behavior-rejected="
              + string (
                  MapEditor.tryImport (
                      "SIR-MAP 2\nsize 4 4\nzone 1 objective behavior trusted\n"
                  )
                  |> Result.isError
              ) ]
    let expectedZoneFixture =
        File.ReadAllText(
            Path.Combine(
                __SOURCE_DIRECTORY__,
                "fixtures",
                "map-editor-milestone-7-zones.txt"
            )
        ).TrimEnd()
    require
        (zoneFixture = expectedZoneFixture
         && zoneAuthored.Revision.Number = editor.Revision.Number + 5L
         && zoneAuthored.UndoHistory.Length = 5
         && zoneResizeLoss.LostRegions = 2
         && zoneRedone.Map = zoneAuthored.Map
         && invalidRegionCodes =
            [ "REGION-POLYGON-AREA"
              "REGION-POLYGON-SELF-INTERSECTION" ])
        ("The deterministic Milestone 7 zone review fixture changed.\n"
         + zoneFixture)

    let maximumMap =
        { editor.Map with
            Width = 40
            Height = 40
            Terrain = Map.empty
            Edges = Map.empty
            Units = Map.empty
            NextUnitId = 1 }
    let maximumState =
        { editor with
            Gesture =
                CommandPreviewGesture(
                    ReplaceDocument("maximum-map-performance", maximumMap)
                ) }
        |> MapEditor.update CommitEditorGesture
    let maximumGestureTimings =
        Array.init 80 (fun index ->
            let tool = if index % 2 = 0 then FloodFillTool else LineTool
            let started = Diagnostics.Stopwatch.GetTimestamp()
            maximumState
            |> MapEditor.update (ChooseTerrain Rough)
            |> MapEditor.update (ChooseTool(Terrain tool))
            |> MapEditor.update (BeginTerrainGesture(address 0 0))
            |> MapEditor.update (ExtendTerrainGesture(address 39 39))
            |> MapEditor.terrainPreview
            |> ignore
            let elapsed = Diagnostics.Stopwatch.GetTimestamp() - started
            float elapsed * 1000.0 / float Diagnostics.Stopwatch.Frequency)
        |> Array.sort
    let maximumGestureP95 =
        maximumGestureTimings[int (float maximumGestureTimings.Length * 0.95)]
    require
        (maximumState.Map.Width = 40
         && maximumState.Map.Height = 40
         && maximumGestureP95 < 50.0)
        "Maximum-map terrain preview and command validation exceeded the 50 ms p95 guardrail."
    printfn
        "Maximum-map terrain evidence: 40x40 cells, flood-fill/line preview plus validation p95 %.3f ms over %d alternating gestures."
        maximumGestureP95
        maximumGestureTimings.Length

    let performanceFrame = Battlefield.performanceFrame 200
    for _ in 1 .. 20 do
        Battlefield.scene performanceFrame Battlefield.initial |> ignore

    let sceneTimings =
        Array.init 240 (fun _ ->
            let started = Diagnostics.Stopwatch.GetTimestamp()
            Battlefield.scene performanceFrame Battlefield.initial |> ignore
            let elapsed = Diagnostics.Stopwatch.GetTimestamp() - started
            float elapsed * 1000.0 / float Diagnostics.Stopwatch.Frequency)
        |> Array.sort

    let p95SceneMilliseconds = sceneTimings[int (float sceneTimings.Length * 0.95)]
    require
        (p95SceneMilliseconds < 8.0)
        "The 200-unit pure scene projection exceeded the 8 ms p95 frame-work budget."

    let stressFrame = Battlefield.performanceFrame 400
    let stressScene = Battlefield.scene stressFrame Battlefield.initial
    let stressTimings =
        Array.init 120 (fun _ ->
            let started = Diagnostics.Stopwatch.GetTimestamp()
            Battlefield.scene stressFrame Battlefield.initial |> ignore
            let elapsed = Diagnostics.Stopwatch.GetTimestamp() - started
            float elapsed * 1000.0 / float Diagnostics.Stopwatch.Frequency)
        |> Array.sort
    let p95StressMilliseconds =
        stressTimings[int (float stressTimings.Length * 0.95)]

    let performanceProvenance frame =
        { SourceIdentity = "performance-fixture"
          ReplayIdentity = "none"
          ProjectionIdentity = EvidenceExport.projectionIdentity frame
          EngineIdentity = "test-engine"
          RulesetIdentity = None
          Tick = frame.Tick
          Mode = DerivedSimulationEvidence
          PaletteIdentity = ReplayPalettes.accessibleDefault.Id
          RendererVersion = EvidenceExport.RendererVersion }
    let exportTimings frame runs =
        Array.init runs (fun _ ->
            let started = Diagnostics.Stopwatch.GetTimestamp()
            EvidenceExport.svg (performanceProvenance frame) None frame |> ignore
            let elapsed = Diagnostics.Stopwatch.GetTimestamp() - started
            float elapsed * 1000.0 / float Diagnostics.Stopwatch.Frequency)
        |> Array.sort
    let normalExportTimings = exportTimings performanceFrame 80
    let stressExportTimings = exportTimings stressFrame 40
    let p95NormalExport =
        normalExportTimings[int (float normalExportTimings.Length * 0.95)]
    let p95StressExport =
        stressExportTimings[int (float stressExportTimings.Length * 0.95)]
    require
        (stressScene.Units.Length = 400
         && p95NormalExport < 100.0
         && p95StressExport < 250.0)
        "Stress projection or deterministic evidence export exceeded its review guardrail."

    printfn
        "Static battlefield performance: 200 units, %d estimated interactive nodes, pure scene projection p95 %.3f ms over %d runs; 400-unit stress projection p95 %.3f ms over %d runs; safe SVG export p95 %.3f ms normal / %.3f ms stress."
        performanceScene.InteractiveNodeEstimate
        p95SceneMilliseconds
        sceneTimings.Length
        p95StressMilliseconds
        stressTimings.Length
        p95NormalExport
        p95StressExport

    printfn "Elmish, map-editor, laboratory, render-contract, glyph-catalog, palette, and static-battlefield tests passed: deterministic map import/export, manual/scripted/general controllers, placement validation, deterministic update, modes, bounded worker batches, compact progress, failure revocation, immutable baseline, typed validation, deterministic sweep, reproducible fixture export, sandbox, stale responses, cancellation, disclosure-safe transport, complete initial class coverage, safe placeholder, three accessible palette modes, orthographic footprints, twelve-segment health, elevation, stance, semantic-zoom hysteresis, roving focus, exact committed evidence, and a 200-unit view under the node budget."
    0
