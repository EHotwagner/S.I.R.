module Program

open System

[<EntryPoint>]
let main argv =
    let quick = argv |> Array.contains "--quick"

    printfn "S.I.R. WASM invocation spike"
    printfn "runtime: %s" (Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
    printfn "os:      %s" (Runtime.InteropServices.RuntimeInformation.OSDescription)
    printfn "arch:    %A" Runtime.InteropServices.RuntimeInformation.OSArchitecture
    printfn "cores:   %d" Environment.ProcessorCount
    printfn "gc:      server=%b concurrent=%b"
        Runtime.GCSettings.IsServerGC
        (Runtime.GCSettings.LatencyMode <> Runtime.GCLatencyMode.Batch)
    printfn ""
    printfn "Question: can the authoritative server invoke one WASM instance per"
    printfn "unit, every tick, at 100 units per side, inside a 50 ms budget?"

    // 200 units = 100 per side, the intended upper force target.
    let units = 200
    let contacts = 30
    let work = 4

    let engine, art, instances = Scenarios.compileAndInstantiate units
    Scenarios.tickCostBreakdown instances contacts work 10_000_000UL (if quick then 200 else 1000)
    |> ignore
    for u in instances do u.Store.Dispose()
    engine.Dispose()

    Scenarios.correctnessChecks ()
    Scenarios.unitCountSweep contacts work 100_000_000UL
    Scenarios.fuelSweep units contacts
    Scenarios.observationSweep units work
    Scenarios.parallelExecution units contacts work
    Scenarios.marshallingStrategies units work

    if not quick then
        Scenarios.sustainedMatch units contacts work 60
        Scenarios.stress ()

    printfn ""
    printfn "%s" (String.replicate 78 "-")
    printfn "done"
    0
