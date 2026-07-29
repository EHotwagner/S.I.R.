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

        let numeric = NumericFixtures.evaluate injectAt

        match NumericFixtures.firstDivergence numeric with
        | Some divergence ->
            eprintfn
                "first divergence: fixture=%s byte=%d expected=%02x actual=%02x"
                divergence.FixtureName
                divergence.ByteOffset
                divergence.Expected
                divergence.Actual

            failwith "Canonical conformance failed."
        | None ->
            let simulation = SimulationFixtures.evaluate None

            match SimulationFixtures.firstDivergence simulation with
            | Some divergence ->
                eprintfn
                    "first divergence: tick=%d phase=%s byte=%d expected=%02x actual=%02x"
                    divergence.Tick
                    (SimulationFixtures.phaseName divergence.Phase)
                    divergence.ByteOffset
                    divergence.Expected
                    divergence.Actual

                failwith "Simulation conformance failed."
            | None ->
                let replay = ReplayFixtures.evaluate ()
                let orientation = OrientationFixtures.evaluate ()
                let mapScale = SimulationFixtures.mapScaleEvidence ()

                [ NumericFixtures.canonicalBytes numeric
                  orientation
                  SimulationFixtures.canonicalBytes simulation
                  mapScale
                  replay ]
                |> SIR.Domain.CanonicalEncoding.concatenate
                |> NumericFixtures.hex
                |> printfn "%s"

                0
    | [ "--inject-simulation-divergence"; phaseName ] ->
        match SimulationFixtures.tryParsePhase phaseName with
        | None ->
            eprintfn "Unknown simulation phase: %s" phaseName
            2
        | Some phase ->
            let evaluated = SimulationFixtures.evaluate (Some phase)

            match SimulationFixtures.firstDivergence evaluated with
            | Some divergence ->
                eprintfn
                    "first divergence: tick=%d phase=%s byte=%d expected=%02x actual=%02x"
                    divergence.Tick
                    (SimulationFixtures.phaseName divergence.Phase)
                    divergence.ByteOffset
                    divergence.Expected
                    divergence.Actual

                failwith "Simulation conformance failed."
            | None ->
                failwith "The deliberately changed simulation checkpoint was accepted."
    | [ "--print-simulation-oracle" ] ->
        SimulationFixtures.evaluate None
        |> List.iter (fun (fixture, actual) ->
            printfn
                "%s=%s"
                (SimulationFixtures.phaseName fixture.Phase)
                (NumericFixtures.hex actual))

        0
    | [ "--print-replay-evidence" ] ->
        let packageBytes = ReplayFixtures.canonicalPackageBytes ()
        let replayVector = ReplayFixtures.evaluate ()

        printfn "package-bytes=%d" packageBytes.Length
        printfn "package-sha256=%s" (NumericFixtures.hex replayVector[0..31])
        printfn "final-state-sha256=%s" (NumericFixtures.hex replayVector[32..63])
        printfn "perspective-package-sha256=%s" (NumericFixtures.hex replayVector[64..95])
        0
    | _ ->
        eprintfn
            "Usage: conformance [--inject-divergence FIXTURE | --inject-simulation-divergence PHASE | --print-simulation-oracle | --print-replay-evidence]"

        2
