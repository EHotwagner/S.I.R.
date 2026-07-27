module Program

open System
open World

[<EntryPoint>]
let main argv =
    let quick = argv |> Array.contains "--quick"

    printfn "S.I.R. movement spike"
    printfn "runtime: %s" (Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
    printfn "cores:   %d" Environment.ProcessorCount
    printfn ""
    printfn "Question: what do cooperative footprint reservation, conflict"
    printfn "resolution and path search cost at 100 units per side?"

    let g = create 512 512 1
    generateUrban g 7UL
    let p = Clearance.build g 2

    Scenarios.mapProfile g p
    Scenarios.openMovement g 200 2 (if quick then 300 else 1200) |> ignore
    Scenarios.unitSweep g 2
    Scenarios.pathfindingCost g 2
    Scenarios.replanBudget g 200 2
    if not quick then
        Scenarios.footprintSweep g
        // congestion mutates the map, so it runs last on its own copy
        let g2 = create 512 512 1
        generateUrban g2 7UL
        // a single 3-cell gap in a 512-long barrier is a genuinely hard search
        Scenarios.maxExpand <- 400000
        Scenarios.congestion g2 200 2 600

    printfn ""
    printfn "%s" (String.replicate 78 "-")
    printfn "done"
    0
