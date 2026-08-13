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
    let private rulesetHash = CombatRules.packageIdentity.ManifestDigest

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
                  VisibleSamples = 1
                  TotalSamples = 1
                  RangeCells = 1
                  Suppression = FixedPoint.zero
                  BaseDamage = FixedPoint.fromRatio 25 1 |> Result.defaultWith (fun _ -> failwith "invalid damage")
                  ArmorRetention = FixedPoint.fromRatio 4 5 |> Result.defaultWith (fun _ -> failwith "invalid retention")
                  EventId = "replay-v3-attack" }
            |> Result.defaultWith failwith
        Replay.createRulesArchive
            CombatRules.packageIdentity
            CombatRules.registry
            [ Rules.canonicalApplicationBytes attack.Explanation ]

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

    let private combatSnapshot () =
        let redId = Simulation.unitId 10
        let red = Simulation.initialState.Units[redId]
        let wounded =
            { red with
                Armor =
                    { FrontRating = 61
                      RearRating = 23
                      Integrity = 74 }
                Wounds =
                    [ { AttackId = "replay-v3-serious"
                        Severity = WoundSeverity.Serious
                        Damage = 17 }
                      { AttackId = "replay-v3-critical"
                        Severity = WoundSeverity.Critical
                        Damage = 41 } ]
                Incapacitated = true
                Suppression = 37 }
        let cover =
            { CoverId = "stone-wall"
              Cell = { Col = 1; Row = 0 }
              Integrity = 63
              ProjectileBlocking = true }
        { Simulation.initialState with
            Units = Simulation.initialState.Units |> Map.add redId wounded
            Board =
                { Simulation.initialState.Board with
                    Covers = Map.ofList [ cover.CoverId, cover ] } }

    let private combatSnapshotPackage () =
        let state = combatSnapshot ()
        let final =
            { Tick = state.Tick
              OutcomeCode = 0
              StateHash = Replay.stateHash state
              EventHash = Replay.emptyEventHash }
        { FormatVersion = int32 Replay.CurrentFormatVersion
          EngineHash = engineHash
          RulesetHash = rulesetHash
          FullReplayAuthorized = true
          RulesArchive = Some(rulesArchive ())
          Content =
            AuthorizedFullReplay
                { InitialSnapshot = state
                  OrderedInputs = []
                  AcceptedWasmOutputs = []
                  Checkpoints = [ checkpoint state.Tick state [] ]
                  FinalResult = final } }

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
                     && unit.AttentionDirection = North
                     && unit.Armor = { FrontRating = 0; RearRating = 0; Integrity = 0 }
                     && unit.Wounds.IsEmpty
                     && not unit.Incapacitated
                     && unit.Suppression = 0)
                    "Legacy replay orientation/combat defaults were not deterministic.")
            require legacy.InitialSnapshot.Board.Covers.IsEmpty "Legacy replay cover defaults were not deterministic."
        | PerspectivePlayback _ ->
            failwith "Legacy full replay changed disclosure kind."

        let directionalDecoded =
            { package with
                FormatVersion = Replay.DirectionalFormatVersion
                RulesArchive = None }
            |> Replay.encode
            |> Replay.decode Replay.defaultLimits
            |> Result.defaultWith (fun error ->
                failwithf "Directional replay did not import: %A" error)
        match directionalDecoded.Content with
        | AuthorizedFullReplay directional ->
            directional.InitialSnapshot.Units
            |> Map.iter (fun _ unit ->
                require
                    (unit.Armor = { FrontRating = 0; RearRating = 0; Integrity = 0 }
                     && unit.Wounds.IsEmpty
                     && not unit.Incapacitated
                     && unit.Suppression = 0)
                    "Replay v2 combat defaults were not deterministic.")
            require directional.InitialSnapshot.Board.Covers.IsEmpty "Replay v2 cover defaults were not deterministic."
        | PerspectivePlayback _ -> failwith "Replay v2 full replay changed disclosure kind."

        let combatPackage = combatSnapshotPackage ()
        let combatBytes = Replay.encode combatPackage
        let decodedCombat =
            Replay.decode Replay.defaultLimits combatBytes
            |> Result.defaultWith (fun error -> failwithf "Replay v3 combat snapshot did not decode: %A" error)
        require (Replay.encode decodedCombat = combatBytes) "Replay v3 combat snapshot did not round-trip byte-exactly."
        match decodedCombat.Content with
        | AuthorizedFullReplay full ->
            let expectedSnapshotBytes = Replay.snapshotBytes (combatSnapshot ())
            require (Replay.snapshotBytes full.InitialSnapshot = expectedSnapshotBytes) "Replay v3 did not retain the complete combat snapshot."
            require (Replay.snapshotBytes full.Checkpoints.Head.State = expectedSnapshotBytes) "Replay v3 seek point lost combat state."
            let retained = full.InitialSnapshot.Units[Simulation.unitId 10]
            require (retained.Armor = { FrontRating = 61; RearRating = 23; Integrity = 74 }) "Replay v3 lost armor state."
            require (retained.Wounds.Length = 2 && retained.Wounds.Head.AttackId = "replay-v3-serious") "Replay v3 lost wound state."
            require (retained.Incapacitated && retained.Suppression = 37) "Replay v3 lost incapacity or suppression state."
            require (full.InitialSnapshot.Board.Covers["stone-wall"].Integrity = 63) "Replay v3 lost cover state."
        | PerspectivePlayback _ -> failwith "Replay v3 combat snapshot changed disclosure kind."
        match Replay.runKernelReplay Replay.defaultLimits engineHash decodedCombat with
        | Ok(BrowserKernelVerified result) ->
            require (result.StateHash = Replay.stateHash (combatSnapshot ())) "Replay v3 combat seek verification returned the wrong hash."
        | result -> failwithf "Replay v3 combat seek verification failed: %A" result

        let noCoverLimit = { Replay.defaultLimits with MaxCovers = 0 }
        expectError
            (function ResourceLimitExceeded("covers", 1, 0) -> true | _ -> false)
            (Replay.runKernelReplay noCoverLimit engineHash decodedCombat)
            "Replay v3 ignored its retained-cover resource limit."
        let oneWoundLimit = { Replay.defaultLimits with MaxWoundsPerUnit = 1 }
        expectError
            (function ResourceLimitExceeded("wounds per unit", 2, 1) -> true | _ -> false)
            (Replay.runKernelReplay oneWoundLimit engineHash decodedCombat)
            "Replay v3 ignored its retained-wound resource limit."
        expectError
            (function MalformedPackage detail when detail.Contains "Resource limit exceeded for covers" -> true | _ -> false)
            (Replay.decode noCoverLimit combatBytes)
            "Replay v3 decoded covers beyond its resource limit."
        expectError
            (function MalformedPackage detail when detail.Contains "Resource limit exceeded for wounds per unit" -> true | _ -> false)
            (Replay.decode oneWoundLimit combatBytes)
            "Replay v3 decoded wounds beyond its per-unit resource limit."

        let originalCombatHash = Replay.stateHash (combatSnapshot ())
        let requireHashMutation label (change: SimulationState -> SimulationState) =
            let changed = change (combatSnapshot ())
            require (Replay.stateHash changed <> originalCombatHash) ("Replay v3 state hash ignored " + label + ".")
        let redId = Simulation.unitId 10
        let changeRed (change: UnitState -> UnitState) (state: SimulationState) =
            let red = state.Units[redId]
            { state with Units = state.Units |> Map.add redId (change red) }
        requireHashMutation "armor" (changeRed (fun unit -> { unit with Armor = { unit.Armor with Integrity = unit.Armor.Integrity - 1 } }))
        requireHashMutation "wounds" (changeRed (fun unit -> { unit with Wounds = unit.Wounds.Tail }))
        requireHashMutation "incapacitation" (changeRed (fun unit -> { unit with Incapacitated = not unit.Incapacitated }))
        requireHashMutation "suppression" (changeRed (fun unit -> { unit with Suppression = unit.Suppression + 1 }))
        requireHashMutation "covers" (fun state -> { state with Board = { state.Board with Covers = Map.empty } })

        let staleSeekHash =
            decodedCombat
            |> mapFull (fun full ->
                let changedState =
                    full.Checkpoints.Head.State
                    |> changeRed (fun unit -> { unit with Suppression = unit.Suppression + 1 })
                { full with
                    Checkpoints =
                        [ { full.Checkpoints.Head with
                              State = changedState } ] })
        expectError
            (function InvalidCheckpoint(0, detail) when detail.Contains "state hash" -> true | _ -> false)
            (Replay.runKernelReplay Replay.defaultLimits engineHash staleSeekHash)
            "Replay v3 accepted a combat-mutated seek snapshot under its retained hash."

        let physicalCodecPackage =
            package
            |> mapFull (fun full ->
                { full with
                    OrderedInputs =
                        [ input 1 1 (PhysicalAttack(Simulation.unitId 10, { Col = 2; Row = 0 }, WeaponProfile.AntiArmor)) ] })
        let physicalCodecBytes = Replay.encode physicalCodecPackage
        let physicalCodecDecoded =
            Replay.decode Replay.defaultLimits physicalCodecBytes
            |> Result.defaultWith (fun error -> failwithf "Replay v3 physical input did not decode: %A" error)
        require (Replay.encode physicalCodecDecoded = physicalCodecBytes) "Replay v3 physical input did not round-trip byte-exactly."

        let decoded =
            match Replay.decode Replay.defaultLimits encoded with
            | Ok decoded -> decoded
            | Error error -> failwithf "Canonical replay did not decode: %A" error

        match decoded.RulesArchive with
        | None -> failwith "Replay v3 omitted its canonical rules archive."
        | Some archive ->
            require (archive = rulesArchive ()) "Replay v3 did not retain exact typed rule-package/application identity."
            let historicalRules =
                Replay.resolveRulesArchive archive
                |> Result.defaultWith (fun detail -> failwith ("Replay v3 could not resolve its embedded historical rules: " + detail))
            let damage =
                historicalRules
                |> List.find (fun rule -> RuleId.value rule.Metadata.Id = "COMBAT-DAMAGE-001")
            require (not (System.String.IsNullOrWhiteSpace damage.Metadata.Rationale)) "Historical damage rationale was not retained."
            match damage.Semantics, damage.Metadata.RuleSource with
            | FormulaSemantics _, Some source ->
                require (source.RepositoryPath = "src/SIR.Simulation/CombatRules.fs") "Historical damage source path was not retained."
                require (source.Commit = archive.Identity.SourceCommit) "Historical damage source commit was not retained."
            | _ -> failwith "Historical damage formula/source metadata was not retained."

            let oldCommit = "1111111111111111111111111111111111111111"
            let historicalRegistry =
                CombatRules.registry
                |> List.map (fun rule ->
                    let historicalSource =
                        rule.Metadata.RuleSource
                        |> Option.map (fun source -> { source with Commit = oldCommit })
                    let historicalRationale =
                        if RuleId.value rule.Metadata.Id = "COMBAT-DAMAGE-001" then
                            "Retained historical damage rationale."
                        else rule.Metadata.Rationale
                    { rule with
                        Metadata =
                            { rule.Metadata with
                                Rationale = historicalRationale
                                RuleSource = historicalSource } })
            let historicalIdentity =
                Rules.packageIdentity
                    archive.Identity.EngineIdentity
                    archive.Identity.CompatibilityProfile
                    archive.Identity.PackageVersion
                    oldCommit
                    CombatRules.implementationArtifacts
                    historicalRegistry
            let historicalArchive =
                Replay.createRulesArchive historicalIdentity historicalRegistry []
            let historicalDamage =
                Replay.resolveRulesArchive historicalArchive
                |> Result.defaultWith failwith
                |> List.find (fun rule -> RuleId.value rule.Metadata.Id = "COMBAT-DAMAGE-001")
            require (historicalDamage.Metadata.Rationale = "Retained historical damage rationale.") "Current rationale replaced replay-owned historical metadata."
            match historicalDamage.Metadata.RuleSource with
            | Some source -> require (source.Commit = oldCommit) "Current source revision replaced the replay-owned historical revision."
            | None -> failwith "Replay-owned historical source metadata became unavailable."

        let expectMalformedArchive label change =
            let changedArchive = rulesArchive () |> change
            let changed = { package with RulesArchive = Some changedArchive }
            expectError
                (function MalformedPackage _ -> true | _ -> false)
                (changed |> Replay.encode |> Replay.decode Replay.defaultLimits)
                ("Rules archive accepted invalid " + label + ".")

        let rehash identity applications = Replay.createRulesArchive identity CombatRules.registry applications
        let identity = CombatRules.packageIdentity
        expectMalformedArchive "engine identity" (fun archive -> rehash { identity with EngineIdentity = "" } archive.Applications)
        expectMalformedArchive "compatibility profile" (fun archive -> rehash { identity with CompatibilityProfile = "" } archive.Applications)
        expectMalformedArchive "package version" (fun archive -> rehash { identity with PackageVersion = "" } archive.Applications)
        expectMalformedArchive "source commit" (fun archive -> rehash { identity with SourceCommit = "not-a-sha" } archive.Applications)
        expectMalformedArchive "implementation digest" (fun archive -> rehash { identity with ImplementationDigest = [| 1uy |] } archive.Applications)
        expectMalformedArchive "semantic digest" (fun archive -> rehash { identity with SemanticDigest = [| 1uy |] } archive.Applications)
        expectMalformedArchive "manifest digest" (fun archive -> rehash { identity with ManifestDigest = [| 1uy |] } archive.Applications)
        expectMalformedArchive "application binding" (fun _ -> Replay.createRulesArchive identity CombatRules.registry [ Array.zeroCreate 32 ])
        let changedRule =
            let first = CombatRules.registry.Head
            { first with Metadata = { first.Metadata with Rationale = first.Metadata.Rationale + " tampered" } }
            :: CombatRules.registry.Tail
        expectMalformedArchive
            "canonical manifest identity"
            (fun archive -> Replay.createRulesArchive identity changedRule archive.Applications)

        let missingArchive = { package with RulesArchive = None }
        expectError
            (function MalformedPackage detail when detail.Contains "requires a rules archive" -> true | _ -> false)
            (Replay.runKernelReplay Replay.defaultLimits engineHash missingArchive)
            "Replay v3 full package accepted a missing rules archive."

        let mismatchedRuleset = { decoded with RulesetHash = hashSeed 65 }
        expectError
            (function MalformedPackage detail when detail.Contains "ruleset hash" -> true | _ -> false)
            (Replay.runKernelReplay Replay.defaultLimits engineHash mismatchedRuleset)
            "Replay v3 accepted a rules archive whose manifest identity differs from the package ruleset."

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
