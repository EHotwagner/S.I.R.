module ScenarioCatalogQualification

open System
open SIR.Client

let private require condition message = if not condition then failwith message

type private StressObservation =
    { MapWidth: int
      MapHeight: int
      Units: int
      Edges: int
      Zones: int
      SimulationTicks: int
      Events: int
      Checkpoints: int
      PathExpansions: int
      PeakLosSamples: int
      PeakCombatResolutions: int
      ProjectionFrames: int
      SceneNodes: int }

let run () =
    let runPackage package =
        ExperienceSamples.importPackage (ExperienceSamples.encodePackage package)
        |> Result.defaultWith (fun errors -> failwith (string errors)) |> ignore
        ExperienceSamples.replayFrames package.Replay |> Array.tryLast |> ignore
    let runUpdateView package =
        ExperienceSamples.importPackage (ExperienceSamples.encodePackage package)
        |> Result.defaultWith (fun errors -> failwith (string errors)) |> ignore
        ExperienceSamples.replayFrames { package.Replay with Ticks = 1 } |> Array.tryLast |> ignore
    ExperienceSamples.packages |> List.iter runPackage
    let humanClasses = Set.ofList [ "rifleman"; "gunner"; "marksman"; "engineer"; "medic"; "signaller" ]
    let canonicalHumanBases sample =
        let editor = ExperienceSamples.editorState sample
        editor.Validation.IsNone
        && (editor.Map.Units
            |> Map.forall (fun _ unit ->
                not (Set.contains unit.ClassId humanClasses) || unit.Size = 4))
    require
        (ExperienceSamples.maps |> List.forall canonicalHumanBases
         && canonicalHumanBases ExperienceSamples.legacyTrollAssault)
        "A visible experience sample no longer uses the canonical 4x4 human footprint."
    let package = ExperienceSamples.packages |> List.head
    let changedReplay =
        { package.Replay with
            Title = package.Replay.Title + " (fork)"
            Summary = package.Replay.Summary + " Forked replay notes." }
    let staleReplayMetadata = { package with Replay = changedReplay }
    require
        (ExperienceSamples.digest staleReplayMetadata <> package.Identity.ContentDigest)
        "Replay title and summary changes did not alter the canonical content digest."
    match ExperienceSamples.validate staleReplayMetadata with
    | Error errors when errors |> List.exists (function StaleContentDigest _ -> true | _ -> false) -> ()
    | result -> failwith ("Replay metadata subject mutation was not rejected with its stale digest: " + string result)
    let reboundReplayMetadata =
        { staleReplayMetadata with
            Identity = { staleReplayMetadata.Identity with ContentDigest = ExperienceSamples.digest staleReplayMetadata } }
    let importedReplayMetadata =
        ExperienceSamples.importPackage (ExperienceSamples.encodePackage reboundReplayMetadata)
        |> Result.defaultWith (fun errors -> failwith ("Replay metadata round-trip failed: " + string errors))
    require
        (importedReplayMetadata = reboundReplayMetadata)
        "Replay title and summary were not preserved by the canonical package round-trip."
    let malformedUnitPackage =
        { package with
            Map = { package.Map with MapText = package.Map.MapText.Replace("unit 1 ", "unit x ") } }
    match ExperienceSamples.validate malformedUnitPackage with
    | Error errors when errors |> List.exists (function MalformedScenarioPackage message -> message.Contains("unit identifier") | _ -> false) -> ()
    | result -> failwith ("Malformed unit identifier escaped the typed package-validation boundary: " + string result)
    let runStressRoute () =
        let stress = ExperienceSamples.stressPackage ()
        let samples = ResizeArray<float>()
        let timed action =
            let timer = Diagnostics.Stopwatch.StartNew()
            let value = action ()
            timer.Stop()
            samples.Add timer.Elapsed.TotalMilliseconds
            value
        timed (fun () ->
            MapEditor.tryImport stress.Map.MapText
            |> Result.defaultWith (fun error -> failwith ("The serialized 80x80 stress fixture failed production import: " + error))
            |> ignore)
        let editor = ExperienceSamples.editorState stress.Map
        let initial =
            ExperienceSamples.simulator stress.Map
            |> Option.defaultWith (fun () -> failwith "The 80x80 stress map did not enter the production simulator.")
        let selected = Some 1
        let selectedUnit = Map.find 1 initial.RuntimeMap.Units
        let path =
            timed (fun () ->
                MapEditorSimulator.preview
                    selected
                    { CellColumn = selectedUnit.Column
                      CellRow = editor.Map.Height - selectedUnit.Size }
                    initial
                |> Option.defaultWith (fun () -> failwith "The production route preview returned no 80x80 path result."))
        let mutable current = initial
        let handoffs = ResizeArray<SimulatorHandoff>()
        let scenes = ResizeArray<BattlefieldScene>()
        handoffs.Add current
        scenes.Add(timed (fun () -> Battlefield.scene (MapEditorSimulator.frame selected current) Battlefield.initial))
        for _ in 1 .. stress.Replay.Ticks do
            let handoff, scene =
                timed (fun () ->
                    let next = MapEditorSimulator.update StepSimulator selected current
                    next, Battlefield.scene (MapEditorSimulator.frame selected next) Battlefield.initial)
            current <- handoff
            handoffs.Add handoff
            scenes.Add scene
        let frames = handoffs |> Seq.map (MapEditorSimulator.frame selected) |> Seq.toList
        let cost = ExperienceSamples.catalogCost [ stress ]
        { MapWidth = editor.Map.Width
          MapHeight = editor.Map.Height
          Units = stress.Forces.Length
          Edges = cost.EdgeCount
          Zones = cost.ZoneCount
          SimulationTicks = stress.Replay.Ticks
          Events = handoffs |> Seq.sumBy (fun handoff -> handoff.LastEvents.Length)
          Checkpoints = stress.ExpectedCheckpoints.Length
          PathExpansions = path.PathExpansions
          PeakLosSamples = handoffs |> Seq.map (fun handoff -> int handoff.LastCounters.LosSamples) |> Seq.max
          PeakCombatResolutions = handoffs |> Seq.map (fun handoff -> int handoff.LastCounters.CombatResolutions) |> Seq.max
          ProjectionFrames = frames.Length
          SceneNodes = scenes |> Seq.map _.InteractiveNodeEstimate |> Seq.max }, List.ofSeq samples
    // Warm up the exact production stress route before either counters or timings are retained.
    let stress = ExperienceSamples.stressPackage ()
    for _ in 1 .. 20 do runStressRoute () |> ignore
    let observation, _ = runStressRoute ()
    require
        (stress.Map.MapText.Contains("size 80 80")
         && observation.Units = 200
         && observation.MapWidth = 80
         && observation.MapHeight = 80
         && observation.ProjectionFrames = 9
         && observation.PathExpansions > 0
         && observation.PeakLosSamples > 0
         && observation.PeakCombatResolutions > 0
         && observation.SceneNodes > observation.Units)
        ("The 80x80/200-unit production-route stress qualification changed: map="
         + string observation.MapWidth + "x" + string observation.MapHeight
         + ", serialized80=" + string (stress.Map.MapText.Contains("size 80 80"))
         + ", forces=" + string observation.Units
         + ", frames=" + string observation.ProjectionFrames
         + ", path=" + string observation.PathExpansions
         + ", los=" + string observation.PeakLosSamples
         + ", combat=" + string observation.PeakCombatResolutions
         + ", scene=" + string observation.SceneNodes + ".")
    require
        (observation.PathExpansions <= 4096
         && observation.PeakLosSamples <= 256
         && observation.PeakCombatResolutions <= 256
         && observation.SceneNodes <= 8000)
        ("The authoritative production path/LOS/combat/scene structural budgets changed: "
         + string observation.PathExpansions + "/" + string observation.PeakLosSamples + "/"
         + string observation.PeakCombatResolutions + "/" + string observation.SceneNodes + ".")
    printfn "Scenario catalog structural counters: scenarios=1 maps=1 map=80x80 units=%d edges=%d zones=%d simulationTicks=%d events=%d checkpoints=%d pathExpansions=%d peakLosSamples=%d peakCombatResolutions=%d projectionFrames=%d sceneNodes=%d."
        observation.Units observation.Edges observation.Zones observation.SimulationTicks observation.Events observation.Checkpoints observation.PathExpansions observation.PeakLosSamples observation.PeakCombatResolutions observation.ProjectionFrames observation.SceneNodes
    let samples =
        [ for _ in 1 .. 20 do
              for package in ExperienceSamples.packages do
                  let timer = Diagnostics.Stopwatch.StartNew()
                  runUpdateView package
                  timer.Stop()
                  yield timer.Elapsed.TotalMilliseconds ]
    let sorted = List.sort samples
    let percentile value = sorted[int (Math.Ceiling(float sorted.Length * value)) - 1]
    let p95, p99 = percentile 0.95, percentile 0.99
    require (p95 <= 20.0 && p99 <= 50.0) ("Scenario catalog workload exceeded 20/50 ms: " + string p95 + "/" + string p99 + ".")
    printfn "Scenario catalog PERF-SMOKE: p95 %.3f ms, p99 %.3f ms, samples [%s]." p95 p99
        (samples |> List.map (fun value -> value.ToString("0.###", Globalization.CultureInfo.InvariantCulture)) |> String.concat ",")
    let stressObservations, stressSamples =
        [ for _ in 1 .. 20 do
              yield runStressRoute () ]
        |> List.unzip
    let stressSamples = List.concat stressSamples
    require
        (stressObservations |> List.forall ((=) observation))
        "The production stress route emitted nondeterministic structural counters."
    let sortedStress = List.sort stressSamples
    let stressPercentile value = sortedStress[int (Math.Ceiling(float sortedStress.Length * value)) - 1]
    let stressP95, stressP99 = stressPercentile 0.95, stressPercentile 0.99
    printfn "Scenario catalog STRESS-SMOKE: workload=scenario-catalog-80x80-200-v1 p95 %.3f ms, p99 %.3f ms, samples [%s]." stressP95 stressP99
        (stressSamples |> List.map (fun value -> value.ToString("0.###", Globalization.CultureInfo.InvariantCulture)) |> String.concat ",")
    // This fixed workload now includes 100 canonical 4x4 humans. Each spatial
    // check covers sixteen cells rather than the former one-cell placeholder.
    require
        (stressP95 <= 60.0 && stressP99 <= 75.0)
        ("Canonical-4x4 scenario catalog stress workload exceeded 60/75 ms: " + string stressP95 + "/" + string stressP99 + ".")
