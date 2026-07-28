module SIR.Conformance.Program

open SIR.Conformance

[<EntryPoint>]
let main arguments =
    match arguments |> Array.toList with
    | []
    | [ "--inject-divergence"; _ ] ->
        let injectAt =
            match arguments |> Array.toList with
            | [ _; fixtureName ] -> Some fixtureName
            | _ -> None

        let evaluated = NumericFixtures.evaluate injectAt

        match NumericFixtures.firstDivergence evaluated with
        | Some divergence ->
            eprintfn
                "first divergence: fixture=%s byte=%d expected=%02x actual=%02x"
                divergence.FixtureName
                divergence.ByteOffset
                divergence.Expected
                divergence.Actual

            failwith "Canonical conformance failed."
        | None ->
            printfn "%s" (evaluated |> NumericFixtures.canonicalBytes |> NumericFixtures.hex)
            0
    | _ ->
        eprintfn "Usage: conformance [--inject-divergence FIXTURE]"
        2
