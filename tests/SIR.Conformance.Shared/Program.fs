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
        let gameCore = GameCoreFixtures.evaluate injectAt

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
            match NumericFixtures.firstDivergence gameCore with
            | Some divergence ->
                eprintfn
                    "first divergence: fixture=%s byte=%d expected=%02x actual=%02x"
                    divergence.FixtureName divergence.ByteOffset divergence.Expected divergence.Actual
                failwith "Game.Core canonical conformance failed."
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
                    let controlAbi = ControlAbiFixtures.evaluate ()
                    let mapScale = SimulationFixtures.mapScaleEvidence ()

                    [ NumericFixtures.canonicalBytes numeric
                      NumericFixtures.canonicalBytes gameCore
                      orientation
                      controlAbi
                      SimulationFixtures.canonicalBytes simulation
                      mapScale
                      RulesCorpusFixtures.evaluate false
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
    | [ "--inject-rules-corpus-divergence" ] ->
        let expected = RulesCorpusFixtures.evaluate false
        let actual = RulesCorpusFixtures.evaluate true
        let offset = Array.zip expected actual |> Array.findIndex (fun (left, right) -> left <> right)
        eprintfn "first divergence: fixture=rules-corpus byte=%d expected=%02x actual=%02x" offset expected[offset] actual[offset]
        failwith "Rules corpus canonical conformance failed."
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
    | [ "--print-rules-manifest" ] ->
        printfn "%s" (RulesCorpusFixtures.manifestJson ())
        0
    | [ "--print-rules-coverage" ] ->
        printfn "%s" (RulesCorpusFixtures.coverageJson ())
        0
    | [ "--print-rules-application" ] ->
        printfn "%s" (RulesCorpusFixtures.representativeApplicationBytes () |> NumericFixtures.hex)
        0
#if !FABLE_COMPILER
    | [ "--print-rules-performance" ] ->
        let stopwatch = System.Diagnostics.Stopwatch.StartNew()
        let checksum, applications, operands, explanationBytes, manifestBytes = RulesCorpusFixtures.performanceWorkload 10_000
        stopwatch.Stop()
        if applications > 32 || operands > 128 || explanationBytes > 65_536 || manifestBytes > 524_288 || stopwatch.ElapsedMilliseconds > 2_000L then
            failwith "Rules corpus performance budget exceeded."
        printfn "iterations=10000"
        printfn "checksum=%d" checksum
        printfn "applications=%d/32" applications
        printfn "operands=%d/128" operands
        printfn "explanation-bytes=%d/65536" explanationBytes
        printfn "manifest-bytes=%d/524288" manifestBytes
        printfn "elapsed-ms=%d/2000" stopwatch.ElapsedMilliseconds
        0
#endif
    | _ ->
        eprintfn
            "Usage: conformance [--inject-divergence FIXTURE | --inject-simulation-divergence PHASE | --inject-rules-corpus-divergence | --print-simulation-oracle | --print-replay-evidence | --print-rules-manifest | --print-rules-coverage | --print-rules-application]"

        2
