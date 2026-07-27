module Program

open System
open World

[<EntryPoint>]
let main argv =
    let quick = argv |> Array.contains "--quick"

    printfn "S.I.R. perception spike"
    printfn "runtime: %s" (Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
    printfn "cores:   %d" Environment.ProcessorCount
    printfn ""
    printfn "Question: what does one tick of perception cost at 100 units per"
    printfn "side, on a 512x512 grid with semantic edges and multiple levels?"

    let units = 200
    let sight = 60

    let g = create 512 512 2
    generateUrban g 7UL

    Scenarios.mapProfile g
    Scenarios.baseline g units sight (if quick then 200 else 1200) |> ignore
    Scenarios.cullingValue g units sight
    Scenarios.unitSweep g sight
    Scenarios.sightSweep g units
    Scenarios.separationSweep g units sight
    if not quick then
        Scenarios.levelSweep units sight
        Scenarios.openTerrain units sight
        Scenarios.sustained g units sight 60
        Scenarios.cachingValue g units sight
        Scenarios.fovCrossover g units sight

    printfn ""
    printfn "%s" (String.replicate 78 "-")
    printfn "done"
    0
