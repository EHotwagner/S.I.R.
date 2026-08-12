module SIR.Conformance.Program

open SIR.Conformance
open SIR.Simulation

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
                      SpatialQueryFixtures.evaluate false
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
    | [ "--inject-spatial-query-divergence" ] ->
        let expected = SpatialQueryFixtures.evaluate false
        let actual = SpatialQueryFixtures.evaluate true
        let offset = Array.zip expected actual |> Array.findIndex (fun (left, right) -> left <> right)
        eprintfn "first divergence: fixture=spatial-query byte=%d expected=%02x actual=%02x" offset expected[offset] actual[offset]
        failwith "Spatial query canonical conformance failed."
    | [ "--print-spatial-query" ] ->
        SpatialQueryFixtures.evaluate false |> NumericFixtures.hex |> printfn "%s"
        0
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
    | [ "--print-replay-package" ] ->
        ReplayFixtures.canonicalPackageBytes () |> NumericFixtures.hex |> printfn "%s"
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
    | [ "--print-spatial-performance" ] ->
        let los, losMs, route, routeMs, invalidationMs, demand100, demand100Ms, demand200, demand200Ms = SpatialQueryFixtures.performanceWorkload ()
        printfn "selected-los-ms=%d/20" losMs
        printfn "route-preview-ms=%d/50" routeMs
        printfn "local-invalidation-ms=%d/10" invalidationMs
        printfn "demand-100-ms=%d/250 expansions=%d" demand100Ms demand100
        printfn "demand-200-ms=%d/500 expansions=%d" demand200Ms demand200
        printfn "route-cells=%d/64 expansions=%d/4096 explanation-bytes=%d/65536" route.Path.Length route.Explanation.Expansions (SpatialQuery.canonicalResultBytes los |> Array.length)
        if los.Explanation.FootprintSamples.Length > 256 || los.Explanation.CrossedCells.Length > 4096 || route.Explanation.Expansions > 4096 || route.Path.Length > 64 || SpatialQuery.canonicalResultBytes los |> Array.length > 65_536 then
            failwith "Spatial query structural performance budget exceeded."
        if losMs > 20L || routeMs > 50L || invalidationMs > 10L || demand100Ms > 250L || demand200Ms > 500L then
            failwith "Spatial query latency performance budget exceeded on the qualification host."
        0
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
            "Usage: conformance [--inject-divergence FIXTURE | --inject-simulation-divergence PHASE | --inject-rules-corpus-divergence | --inject-spatial-query-divergence | --print-spatial-query | --print-simulation-oracle | --print-replay-evidence | --print-rules-manifest | --print-rules-coverage | --print-rules-application]"

        2
