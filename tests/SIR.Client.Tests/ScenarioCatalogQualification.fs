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
    let frames = ExperienceSamples.replayFrames stress.Replay
    let editor = ExperienceSamples.editorState stress.Map
    let events = frames |> Array.sumBy (fun frame -> frame.Events.Length)
    let paths = frames |> Array.sumBy (fun frame -> frame.Events |> List.filter (fun event -> event.Source = "sample-simulation" && event.Summary.Contains("moves")) |> List.length)
    let combat = frames |> Array.sumBy (fun frame -> frame.Events |> List.filter (fun event -> event.Source.StartsWith("combat-", StringComparison.Ordinal)) |> List.length)
    let nodes = frames |> Array.sumBy (fun frame -> frame.Units.Length)
    require (stress.Forces.Length = 200 && editor.Map.Width = 80 && editor.Map.Height = 80 && frames.Length = 9 && nodes = 1800)
        ("The 80x80/200-unit production-route stress qualification changed: " + string editor.Validation + ".")
    require (paths <= 4096 && combat <= 256 && nodes <= 8000) "The declared path/LOS/scene structural budgets changed."
    let cost = ExperienceSamples.catalogCost [ stress ]
    printfn "Scenario catalog structural counters: scenarios=1 maps=1 map=80x80 units=%d edges=%d zones=%d simulationTicks=%d events=%d checkpoints=%d pathExpansions=%d losSamples=%d combatResolutions=%d projectionFrames=%d sceneNodes=%d."
        stress.Forces.Length cost.EdgeCount cost.ZoneCount stress.Replay.Ticks events stress.ExpectedCheckpoints.Length paths combat combat frames.Length nodes
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
