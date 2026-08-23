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
                      AwarenessReactionFixtures.evaluate None
                      CombatFixtures.evaluate false
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
    | [ "--inject-rule-coherence-mutation"; mutation ] ->
        RuleCoherenceFixtures.evaluateProtectedMutation mutation
        0
    | [ "--inject-spatial-query-divergence" ] ->
        let expected = SpatialQueryFixtures.evaluate false
        let actual = SpatialQueryFixtures.evaluate true
        let offset = Array.zip expected actual |> Array.findIndex (fun (left, right) -> left <> right)
        eprintfn "first divergence: fixture=spatial-query byte=%d expected=%02x actual=%02x" offset expected[offset] actual[offset]
        failwith "Spatial query canonical conformance failed."
    | [ "--inject-combat-divergence" ] ->
        let expected = CombatFixtures.evaluate false
        let actual = CombatFixtures.evaluate true
        let offset = Array.zip expected actual |> Array.findIndex (fun (left, right) -> left <> right)
        eprintfn "first divergence: fixture=physical-combat byte=%d expected=%02x actual=%02x" offset expected[offset] actual[offset]
        failwith "Physical combat canonical conformance failed."
    | [ "--inject-awareness-mutation"; mutation ] ->
        AwarenessReactionFixtures.evaluate (Some mutation) |> ignore
        failwith "The awareness/reaction protected-subject mutation was accepted."
    | [ "--inject-replay-mutation"; mutation ] ->
        ReplayFixtures.evaluateProtectedMutation mutation
        failwith "The replay protected-subject mutation was accepted."
    | [ "--print-spatial-query" ] ->
        SpatialQueryFixtures.evaluate false |> NumericFixtures.hex |> printfn "%s"
        0
    | [ "--print-combat" ] ->
        CombatFixtures.evaluate false |> NumericFixtures.hex |> printfn "%s"
        0
    | [ "--print-awareness-reaction" ] ->
        AwarenessReactionFixtures.evaluate None |> NumericFixtures.hex |> printfn "%s"
        0
    | [ "--print-simulation-oracle" ] ->
        let result = Simulation.runTick Simulation.initialState Simulation.inputs
        result.Checkpoints
        |> List.iter (fun checkpoint ->
            let encoded = Simulation.checkpointBytes checkpoint
            let actual = if checkpoint.Phase = SimulationPhase.CommitPhase then SIR.Domain.CanonicalEncoding.concatenate [ encoded; result.StateDigest ] else encoded
            printfn
                "%s=%s"
                (SimulationFixtures.phaseName checkpoint.Phase)
                (NumericFixtures.hex actual))

        0
    | [ "--print-replay-evidence" ] ->
        let packageBytes = ReplayFixtures.canonicalPackageBytes ()
        let replayVector = ReplayFixtures.evaluate ()
        let retainedV3, retainedV4, currentV5 = ReplayFixtures.compatibilityEvidence ()

        printfn "package-bytes=%d" packageBytes.Length
        printfn "package-sha256=%s" (NumericFixtures.hex replayVector[0..31])
        printfn "final-state-sha256=%s" (NumericFixtures.hex replayVector[32..63])
        printfn "perspective-package-sha256=%s" (NumericFixtures.hex replayVector[64..95])
        printfn "retained-v3-bytes=%d sha256=%s" retainedV3.Length (retainedV3 |> SIR.Domain.CanonicalHash.sha256 |> NumericFixtures.hex)
        printfn "physical-v4-bytes=%d sha256=%s" retainedV4.Length (retainedV4 |> SIR.Domain.CanonicalHash.sha256 |> NumericFixtures.hex)
        printfn "awareness-v5-bytes=%d sha256=%s" currentV5.Length (currentV5 |> SIR.Domain.CanonicalHash.sha256 |> NumericFixtures.hex)
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
    | [ "--print-rule-coherence" ] ->
        printfn "%s" (RuleCoherenceFixtures.evaluate () |> NumericFixtures.hex)
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
        // S.I.R.#249 F4 - `neighbours` evaluated once per expansion. Gated on ALLOCATION because
        // that is the only signature the property has: the duplicate evaluation this catches leaves
        // the path, cost, expansions and canonical bytes byte-identical, so no result-level
        // assertion can see it. The counters are printed with it so a change in WHAT the query did
        // is never mistaken for a change in how wastefully it did it.
        let allocated, pathExpansions, pathCells, pathCost, pathOutcome, pathBoundaries = SpatialQueryFixtures.pathAllocationWorkload ()
        printfn "path-allocated-bytes=%d/%d boundaries=%d expansions=%d path-cells=%d cost=%d outcome=%A" allocated 20_000_000L pathBoundaries pathExpansions pathCells pathCost pathOutcome
        // The ceiling sits between the measured correct cost and the measured cost of the specific
        // regression it exists to catch, not at a round number: re-evaluating `neighbours` in the
        // `nextBest` fold raises this by >50%. A bound this far above the true figure also absorbs
        // runtime-version drift, which a byte-exact assertion would turn into a flake.
        if allocated > 20_000_000L then
            failwithf "Spatial bounded-path allocation budget exceeded: %d bytes. `neighbours` is evaluated more than once per expansion in boundedPath's fallback loop." allocated
        if pathOutcome <> SpatialOutcome.Found || pathExpansions <= 0 then
            failwith "Spatial bounded-path allocation gate did not exercise the fallback loop, so its budget proves nothing."
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
    | [ "--print-combat-performance" ] ->
        let final, representativeBytes, representativeMs, stressMs = CombatFixtures.performanceWorkload ()
        printfn "representative-ms=%d/20 bytes=%d" representativeMs representativeBytes
        printfn "stress-units=%d/100 attacks=50 stress-ms=%d/50" final.Combatants.Count stressMs
        printfn "trace-cells=256 area-cells=256 recipients=256 facts=4096 explanation-bytes=65536"
        if final.Combatants.Count <> 100 || representativeMs > 20L || stressMs > 50L then failwith "Physical combat performance budget exceeded."
        0
#endif
    | _ ->
        eprintfn
            "Usage: conformance [--inject-divergence FIXTURE | --inject-simulation-divergence PHASE | --inject-rules-corpus-divergence | --inject-rule-coherence-mutation NAME | --inject-spatial-query-divergence | --inject-combat-divergence | --inject-awareness-mutation NAME | --inject-replay-mutation NAME | --print-spatial-query | --print-combat | --print-awareness-reaction | --print-combat-performance | --print-simulation-oracle | --print-replay-evidence | --print-rules-manifest | --print-rules-coverage | --print-rules-application | --print-rule-coherence]"

        2
