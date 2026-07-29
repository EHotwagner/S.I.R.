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
