namespace SIR.Conformance

open SIR.Domain
open SIR.Simulation

[<RequireQualifiedAccess>]
module ReplayFixtures =
    let private require condition message =
        if not condition then
            failwith message

    let private hashSeed start =
        [| for value in start .. start + 31 -> byte value |]

    let engineHash = hashSeed 1
    let private rulesetHash = hashSeed 65

    let private input tick sequence value: ReplayInput =
        { Tick = tick
          Sequence = sequence
          Input = value }

    let private wasm tick sequence value: AcceptedWasmOutput =
        { Tick = tick
          Sequence = sequence
          Input = value }

    let private checkpoint tick state events =
        { Tick = tick
          State = state
          StateHash = Replay.stateHash state
          EventHash = Replay.eventHash events }

    let private rulesArchive () =
        let attack =
            CombatRules.resolveAttack
                { Attacker = { Col = 0; Row = 0 }
                  TargetFootprint = [ { Col = 1; Row = 0 } ]
                  IsTransparent = fun _ -> true
                  RangeCells = 1
                  Suppression = FixedPoint.zero
                  BaseDamage = FixedPoint.fromRatio 25 1 |> Result.defaultWith (fun _ -> failwith "invalid damage")
                  ArmorRetention = FixedPoint.fromRatio 4 5 |> Result.defaultWith (fun _ -> failwith "invalid retention")
                  EventId = "replay-v3-attack" }
            |> Result.defaultWith failwith
        let identity = CombatRules.packageIdentity
        let text (value: string) = System.Text.Encoding.UTF8.GetBytes value
        CanonicalEncoding.concatenate
            [ text identity.CompatibilityProfile
              text identity.PackageVersion
              text identity.SourceCommit
              identity.ImplementationDigest
              identity.SemanticDigest
              identity.ManifestDigest
              Rules.canonicalApplicationBytes attack.Explanation
              text CombatRules.retainedPackage.ManifestJson
              text CombatRules.retainedPackage.CoverageJson ]

    let private fullPackage () =
        let red = Simulation.unitId 10
        let blue = Simulation.unitId 20

        let externalInputs =
            [ input 1 1 (Move(red, { Col = 1; Row = 1 }))
              input 1 2 (Move(blue, { Col = 1; Row = 0 })) ]

        let wasmOutputs =
            [ wasm 1 3 (Observe(red, blue))
              wasm 1 4 (Attack(red, blue)) ]

        let firstTickJournal =
            (externalInputs |> List.map (fun accepted -> accepted.Input))
            @ (wasmOutputs |> List.map (fun accepted -> accepted.Input))

        let first = Simulation.runTick Simulation.initialState firstTickJournal
        let second = Simulation.runTick first.State []

        let finalResult =
            { Tick = second.State.Tick
              OutcomeCode = 75
              StateHash = Replay.stateHash second.State
              EventHash = Replay.eventHash second.Events }

        { FormatVersion = int32 Replay.CurrentFormatVersion
          EngineHash = engineHash
          RulesetHash = rulesetHash
          FullReplayAuthorized = true
          RulesArchive = Some(rulesArchive ())
          Content =
            AuthorizedFullReplay
                { InitialSnapshot = Simulation.initialState
                  OrderedInputs = externalInputs
                  AcceptedWasmOutputs = wasmOutputs
                  Checkpoints =
                    [ checkpoint 0 Simulation.initialState []
                      checkpoint 1 first.State first.Events ]
                  FinalResult = finalResult } }

    let canonicalPackageBytes () = fullPackage () |> Replay.encode

    let private perspectivePackage =
        { FormatVersion = int32 Replay.CurrentFormatVersion
          EngineHash = engineHash
          RulesetHash = rulesetHash
          FullReplayAuthorized = false
          RulesArchive = None
          Content =
            PerspectivePlayback
                [ { Tick = 1
                    ProjectionHash = CanonicalHash.sha256 [| 1uy; 2uy; 3uy |] } ] }

    let private expectError predicate result message =
        match result with
        | Error error when predicate error -> ()
        | _ -> failwith message

    let private mapFull change package =
        match package.Content with
        | AuthorizedFullReplay full ->
            { package with
                Content = AuthorizedFullReplay(change full) }
        | PerspectivePlayback _ -> failwith "Expected a full replay."

    let evaluate () =
        let package = fullPackage ()
        let encoded = canonicalPackageBytes ()

        let legacyPackage =
            { package with
                FormatVersion = Replay.LegacyFormatVersion
                RulesArchive = None }

        let legacyDecoded =
            legacyPackage
            |> Replay.encode
            |> Replay.decode Replay.defaultLimits
            |> Result.defaultWith (fun error ->
                failwithf "Legacy replay did not import: %A" error)

        match legacyDecoded.Content with
        | AuthorizedFullReplay legacy ->
            legacy.InitialSnapshot.Units
            |> Map.iter (fun _ unit ->
                require
                    (unit.BodyFacing = North
                     && unit.AttentionDirection = North)
                    "Legacy replay orientation defaults were not deterministic.")
        | PerspectivePlayback _ ->
            failwith "Legacy full replay changed disclosure kind."

        let decoded =
            match Replay.decode Replay.defaultLimits encoded with
            | Ok decoded -> decoded
            | Error error -> failwithf "Canonical replay did not decode: %A" error

        match decoded.RulesArchive with
        | None -> failwith "Replay v3 omitted its canonical rules archive."
        | Some archive -> require (archive = rulesArchive ()) "Replay v3 did not retain exact canonical rule-package/application bytes."

        require
            (Replay.encode decoded = encoded)
            "Replay decode/encode did not preserve canonical bytes."

        match Replay.runKernelReplay Replay.defaultLimits engineHash decoded with
        | Ok(BrowserKernelVerified finalResult) ->
            require
                (finalResult.StateHash =
                    (match package.Content with
                     | AuthorizedFullReplay full -> full.FinalResult.StateHash
                     | PerspectivePlayback _ -> failwith "Expected a full replay."))
                "Kernel replay returned the wrong final digest."
        | result ->
            failwithf "The browser kernel runner did not verify the replay: %A" result

        expectError
            (function
            | WasmExecutionNotVerified -> true
            | _ -> false)
            (Replay.verifyAuthoritative
                Replay.defaultLimits
                engineHash
                None
                decoded)
            "Browser replay incorrectly claimed authoritative WASM verification."

        let acceptedOutputs =
            match decoded.Content with
            | AuthorizedFullReplay full -> full.AcceptedWasmOutputs
            | PerspectivePlayback _ -> failwith "Expected a full replay."

        match
            Replay.verifyAuthoritative
                Replay.defaultLimits
                engineHash
                (Some acceptedOutputs)
                decoded
        with
        | Ok(AuthoritativeVerified _) -> ()
        | result -> failwithf "Authoritative replay verification failed: %A" result

        let changedOutputs =
            acceptedOutputs
            |> List.map (fun output ->
                if output.Sequence = 3 then
                    { output with
                        Input =
                            Move(
                                Simulation.unitId 10,
                                { Col = 0; Row = 1 }
                            ) }
                else
                    output)

        expectError
            (function
            | WasmOutputDivergence(1, 3) -> true
            | _ -> false)
            (Replay.verifyAuthoritative
                Replay.defaultLimits
                engineHash
                (Some changedOutputs)
                decoded)
            "Changed WASM re-execution output received an authoritative claim."

        let truncated = encoded[0 .. encoded.Length - 2]

        expectError
            (function
            | MalformedPackage _ -> true
            | _ -> false)
            (Replay.decode Replay.defaultLimits truncated)
            "A truncated replay package was accepted."

        let smallLimit =
            { Replay.defaultLimits with
                MaxPackageBytes = encoded.Length - 1 }

        expectError
            (function
            | PackageTooLarge _ -> true
            | _ -> false)
            (Replay.decode smallLimit encoded)
            "An oversized replay package was accepted."

        let incompatible =
            { decoded with
                FormatVersion = int32 Replay.CurrentFormatVersion + 1 }

        expectError
            (function
            | UnsupportedFormat _ -> true
            | _ -> false)
            (Replay.runKernelReplay Replay.defaultLimits engineHash incompatible)
            "An incompatible replay format was accepted."

        expectError
            (function
            | EngineMismatch _ -> true
            | _ -> false)
            (Replay.runKernelReplay
                Replay.defaultLimits
                (hashSeed 97)
                decoded)
            "A replay for a different engine was accepted."

        let unauthorized =
            { decoded with
                FullReplayAuthorized = false }

        expectError
            (function
            | UnauthorizedFullReplay -> true
            | _ -> false)
            (Replay.runKernelReplay Replay.defaultLimits engineHash unauthorized)
            "An unauthorized full replay was accepted."

        let unordered =
            decoded
            |> mapFull (fun full ->
                { full with
                    OrderedInputs = List.rev full.OrderedInputs })

        expectError
            (function
            | InvalidOrdering "inputs" -> true
            | _ -> false)
            (Replay.runKernelReplay Replay.defaultLimits engineHash unordered)
            "An unordered replay journal was accepted."

        let lowInputLimit =
            { Replay.defaultLimits with
                MaxInputs = 1 }

        expectError
            (function
            | ResourceLimitExceeded("inputs", _, 1) -> true
            | _ -> false)
            (Replay.runKernelReplay lowInputLimit engineHash decoded)
            "An input-count resource limit was ignored."

        let divergent =
            decoded
            |> mapFull (fun full ->
                let changed = Array.copy full.FinalResult.StateHash
                changed[0] <- changed[0] ^^^ 1uy

                { full with
                    FinalResult =
                        { full.FinalResult with
                            StateHash = changed } })

        expectError
            (function
            | ReplayDivergence(2, "final state hash") -> true
            | _ -> false)
            (Replay.runKernelReplay Replay.defaultLimits engineHash divergent)
            "A corrupt final replay digest was accepted."

        let perspectiveBytes = Replay.encode perspectivePackage

        let perspective =
            match Replay.decode Replay.defaultLimits perspectiveBytes with
            | Ok value -> value
            | Error error -> failwithf "Perspective package did not decode: %A" error

        match Replay.runKernelReplay Replay.defaultLimits engineHash perspective with
        | Ok(PerspectiveReady [ frame ]) ->
            require (frame.Tick = 1) "Perspective projection tick changed."
        | result -> failwithf "Perspective playback was not preserved: %A" result

        expectError
            (function
            | PerspectiveHasNoKernel -> true
            | _ -> false)
            (Replay.requireKernel perspective)
            "Perspective playback exposed reconstructable kernel state."

        let abcDigest =
            CanonicalHash.sha256 [| 0x61uy; 0x62uy; 0x63uy |]
            |> NumericFixtures.hex

        require
            (abcDigest =
                "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")
            "Portable SHA-256 failed the published abc vector."

        [ CanonicalHash.sha256 encoded
          (match package.Content with
           | AuthorizedFullReplay full -> full.FinalResult.StateHash
           | PerspectivePlayback _ -> failwith "Expected a full replay.")
          CanonicalHash.sha256 perspectiveBytes ]
        |> CanonicalEncoding.concatenate
