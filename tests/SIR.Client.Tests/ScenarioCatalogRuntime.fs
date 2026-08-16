module SIR.Client.Tests.ScenarioCatalogRuntime

open SIR.Client

[<EntryPoint>]
let main _ =
    ExperienceSamples.packages
    |> List.map (fun package -> package.Map.Id + "=" + ExperienceSamples.runtimeFingerprint package)
    |> String.concat "\n"
    |> printfn "%s"
    0
