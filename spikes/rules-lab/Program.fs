module Program

open System

[<EntryPoint>]
let main argv =
    let quick = argv |> Array.contains "--quick"

    printfn "S.I.R. rules laboratory"
    printfn "runtime: %s" Runtime.InteropServices.RuntimeInformation.FrameworkDescription
    printfn ""
    printfn "Question: which provisional combat parameters preserve the intended"
    printfn "weapon, facing, suppression, and species relationships?"
    printfn ""
    printfn "All formulas and values are non-canonical balance-lab inputs."

    let checks = Scenarios.run quick
    let failed = checks |> List.filter (fun check -> not check.Passed)

    printfn ""
    printfn "%s" (String.replicate 78 "-")
    if List.isEmpty failed then
        printfn "all %d qualitative invariants passed" checks.Length
        0
    else
        printfn "%d of %d qualitative invariants failed" failed.Length checks.Length
        1
