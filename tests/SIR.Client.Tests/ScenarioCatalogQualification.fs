module ScenarioCatalogQualification

open System
open SIR.Client

let private require condition message = if not condition then failwith message

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
    let stress = ExperienceSamples.stressPackage ()
    runPackage stress
    MapEditor.tryImport stress.Map.MapText
    |> Result.defaultWith (fun error -> failwith ("The serialized 80x80 stress fixture failed production import: " + error))
    |> ignore
    let editor = ExperienceSamples.editorState stress.Map
    let initial =
        ExperienceSamples.simulator stress.Map
        |> Option.defaultWith (fun () -> failwith "The 80x80 stress map did not enter the production simulator.")
    let selected = Some 1
    let path =
        MapEditorSimulator.preview selected { CellColumn = 79; CellRow = 79 } initial
        |> Option.defaultWith (fun () -> failwith "The production route preview returned no 80x80 path result.")
    let handoffs =
        [ 1 .. stress.Replay.Ticks ]
        |> List.scan (fun handoff _ -> MapEditorSimulator.update StepSimulator selected handoff) initial
    let scenes =
        handoffs
        |> List.map (fun handoff ->
            Battlefield.scene (MapEditorSimulator.frame selected handoff) Battlefield.initial)
    let frames = ExperienceSamples.replayFrames stress.Replay
    let events = frames |> Array.sumBy (fun frame -> frame.Events.Length)
    let losSamples = handoffs |> List.map (fun handoff -> int handoff.LastCounters.LosSamples) |> List.max
    let combatResolutions = handoffs |> List.map (fun handoff -> int handoff.LastCounters.CombatResolutions) |> List.max
    let sceneNodes = scenes |> List.map _.InteractiveNodeEstimate |> List.max
    require
        (stress.Map.MapText.Contains("size 80 80")
         && stress.Forces.Length = 200
         && editor.Map.Width = 80
         && editor.Map.Height = 80
         && frames.Length = 9
         && path.PathExpansions > 0
         && losSamples > 0
         && combatResolutions > 0
         && sceneNodes > stress.Forces.Length)
        ("The 80x80/200-unit production-route stress qualification changed: map="
         + string editor.Map.Width + "x" + string editor.Map.Height
         + ", serialized80=" + string (stress.Map.MapText.Contains("size 80 80"))
         + ", forces=" + string stress.Forces.Length
         + ", frames=" + string frames.Length
         + ", path=" + string path.PathExpansions
         + ", los=" + string losSamples
         + ", combat=" + string combatResolutions
         + ", scene=" + string sceneNodes
         + ", validation=" + string editor.Validation + ".")
    require
        (path.PathExpansions <= 4096
         && losSamples <= 256
         && combatResolutions <= 256
         && sceneNodes <= 8000)
        ("The authoritative production path/LOS/combat/scene structural budgets changed: "
         + string path.PathExpansions + "/" + string losSamples + "/"
         + string combatResolutions + "/" + string sceneNodes + ".")
    let cost = ExperienceSamples.catalogCost [ stress ]
    printfn "Scenario catalog structural counters: scenarios=1 maps=1 map=80x80 units=%d edges=%d zones=%d simulationTicks=%d events=%d checkpoints=%d pathExpansions=%d peakLosSamples=%d peakCombatResolutions=%d projectionFrames=%d sceneNodes=%d."
        stress.Forces.Length cost.EdgeCount cost.ZoneCount stress.Replay.Ticks events stress.ExpectedCheckpoints.Length path.PathExpansions losSamples combatResolutions frames.Length sceneNodes
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
