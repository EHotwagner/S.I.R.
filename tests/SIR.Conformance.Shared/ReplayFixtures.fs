namespace SIR.Conformance

open FS.GG.Game.Core
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

    let private retainedV3Bytes () =
        System.IO.File.ReadAllText("tests/fixtures/replay/v3-minimal-pre-combat.sirr.base64")
        |> fun value -> value.Trim()
        |> System.Convert.FromBase64String

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
                  EventId = "replay-v4-attack" }
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
                    [ { AttackId = "replay-v4-serious"
                        Severity = WoundSeverity.Serious
                        Damage = 17 }
                      { AttackId = "replay-v4-critical"
                        Severity = WoundSeverity.Critical
                        Damage = 41 } ]
                Incapacitated = true
                Suppression = 37 }
        let cover =
            { CoverId = "stone-wall"
              Cell = { Col = 1; Row = 0 }
              Integrity = 63
              ProjectileBlocking = true
              Material = "legacy"
              PenetrationResistance = 0
              ProtectedDirections = [] }
        { Simulation.initialState with
            Units = Simulation.initialState.Units |> Map.add redId wounded
            Board =
                { Simulation.initialState.Board with
                    Covers = Map.ofList [ cover.CoverId, cover ] } }

    let private combatSnapshotPackage () =
        let state = combatSnapshot ()
        let version = int32 Replay.PhysicalCombatFormatVersion
        let versionStateHash = Replay.stateHashForFormatVersion version state
        let final =
            { Tick = state.Tick
              OutcomeCode = 0
              StateHash = versionStateHash
              EventHash = Replay.emptyEventHash }
        { FormatVersion = version
          EngineHash = engineHash
          RulesetHash = rulesetHash
          FullReplayAuthorized = true
          RulesArchive = Some(rulesArchive ())
          Content =
            AuthorizedFullReplay
                { InitialSnapshot = state
                  OrderedInputs = []
                  AcceptedWasmOutputs = []
                  Checkpoints =
                    [ { Tick = state.Tick
                        State = state
                        StateHash = versionStateHash
                        EventHash = Replay.emptyEventHash } ]
                  FinalResult = final } }

    let private awarenessSnapshot () =
        let observerId = Simulation.unitId 10
        let subjectId = Simulation.unitId 20
        let contact =
            { SubjectId = subjectId
              Level = AwarenessLevel.Acquired
              Acquisition = AwarenessReaction.infantryProfile.IdentificationThreshold
              LastStimulusTick = Some 4
              LastStimulus = None
              LastKnownCell = Some { Col = 1; Row = 0 }
              RetainUntilTick = Some 24
              Reason = AwarenessReason.IdentificationThresholdReached }
        let engagement =
            AwarenessReaction.declareEngagement
                "replay-v5-edge"
                observerId
                (EngagementTarget.GuardedEdge("minimal-east-boundary", 1, Edges.edgeBetween { Col = 1; Row = 0 } { Col = 2; Row = 0 } |> Option.defaultWith (fun () -> failwith "invalid retained v5 edge")))
                East
            |> Result.defaultWith failwith
        { combatSnapshot () with
            Awareness = Map.ofList [ (observerId, subjectId), contact ]
            Engagements = Map.ofList [ observerId, engagement ] }

    let private awarenessSnapshotPackageFor version =
        let state = awarenessSnapshot ()
        let versionStateHash = Replay.stateHashForFormatVersion version state
        let final =
            { Tick = state.Tick
              OutcomeCode = 0
              StateHash = versionStateHash
              EventHash = Replay.emptyEventHash }
        { FormatVersion = int32 version
          EngineHash = engineHash
          RulesetHash = rulesetHash
          FullReplayAuthorized = true
          RulesArchive = Some(rulesArchive ())
          Content =
            AuthorizedFullReplay
                { InitialSnapshot = state
                  OrderedInputs =
                    [ input 1 1 (SetAttention(Simulation.unitId 10, East))
                      input 1 2 (PrepareAreaReaction(Simulation.unitId 10, "replay-v5-area", [ { Col = 2; Row = 0 }; { Col = 1; Row = 0 } ], East))
                      if version >= Replay.CurrentFormatVersion then
                          input 1 3 (SetWeaponPosture(Simulation.unitId 10, WeaponPosture.Prepared))
                          input 1 4 (PrepareUnitReaction(Simulation.unitId 10, "replay-v6-unit", Simulation.unitId 20, East))
                          input 1 5 (PrepareEdgeReaction(Simulation.unitId 10, "replay-v6-edge", Edges.edgeBetween { Col = 1; Row = 0 } { Col = 2; Row = 0 } |> Option.defaultWith (fun () -> failwith "invalid replay edge"), East)) ]
                  AcceptedWasmOutputs = []
                  Checkpoints =
                    [ { Tick = state.Tick
                        State = state
                        StateHash = versionStateHash
                        EventHash = Replay.emptyEventHash } ]
                  FinalResult = final } }

    let private awarenessSnapshotPackage () =
        awarenessSnapshotPackageFor Replay.CurrentFormatVersion

    let compatibilityEvidence () =
        let v3 = retainedV3Bytes ()
        let v4 = combatSnapshotPackage () |> Replay.encode
        let v5 = awarenessSnapshotPackageFor Replay.AwarenessFormatVersion |> Replay.encode
        v3, v4, v5

    let evaluateProtectedMutation mutation =
        match mutation with
        | "version" ->
            let incompatible =
                { awarenessSnapshotPackage () with
                    FormatVersion = int32 Replay.CurrentFormatVersion + 1 }
            match Replay.decode Replay.defaultLimits (Replay.encode incompatible) with
            | Error(UnsupportedFormat _) -> failwith "Replay protected version mutation detected."
            | result -> failwithf "Replay version mutation was accepted: %A" result
        | "hash" ->
            let changed =
                awarenessSnapshotPackage ()
                |> mapFull (fun full ->
                    let first = full.Checkpoints.Head
                    { full with
                        Checkpoints =
                            [ { first with
                                  State = { first.State with Awareness = Map.empty } } ] })
            match Replay.runKernelReplay Replay.defaultLimits engineHash changed with
            | Error(InvalidCheckpoint _) -> failwith "Replay protected hash mutation detected."
            | result -> failwithf "Replay hash mutation was accepted: %A" result
        | "bounds" ->
            let bytes = awarenessSnapshotPackage () |> Replay.encode
            let zero = { Replay.defaultLimits with MaxAwarenessContacts = 0 }
            match Replay.decode zero bytes with
            | Error(MalformedPackage detail) when detail.Contains "awareness contacts" ->
                failwith "Replay protected bounds mutation detected."
            | result -> failwithf "Replay bounds mutation was accepted: %A" result
        | "posture" ->
            let state = awarenessSnapshot ()
            let owner = Simulation.unitId 10
            let changed = { state with Units = state.Units |> Map.change owner (Option.map (fun unit -> { unit with WeaponPosture = WeaponPosture.Prepared })) }
            if Replay.stateHash changed <> Replay.stateHash state then failwith "Replay protected posture mutation detected."
            else failwith "Replay posture mutation was accepted."
        | "cursor" ->
            let state = awarenessSnapshot ()
            if Replay.stateHash { state with AwarenessCursor = state.AwarenessCursor + 1 } <> Replay.stateHash state then failwith "Replay protected cursor mutation detected."
            else failwith "Replay cursor mutation was accepted."
        | "input-vocabulary" ->
            let original = awarenessSnapshotPackage () |> Replay.encode
            let changed =
                awarenessSnapshotPackage ()
                |> mapFull (fun full -> { full with OrderedInputs = full.OrderedInputs |> List.tail })
                |> Replay.encode
            if changed <> original then failwith "Replay protected input-vocabulary mutation detected."
            else failwith "Replay input-vocabulary mutation was accepted."
        | "v5-guarded-edge" ->
            let package = awarenessSnapshotPackageFor Replay.AwarenessFormatVersion
            let encoded = Replay.encode package
            let decoded = Replay.decode Replay.defaultLimits encoded |> Result.defaultWith (fun error -> failwithf "v5 guarded edge did not decode: %A" error)
            match decoded.Content with
            | AuthorizedFullReplay full ->
                match full.InitialSnapshot.Engagements[Simulation.unitId 10].Target with
                | EngagementTarget.GuardedEdge("legacy-guarded-edge", 0, _) when Replay.encode decoded = encoded -> failwith "Replay protected v5 guarded-edge mutation detected."
                | target -> failwithf "v5 guarded edge lost legacy defaults: %A" target
            | _ -> failwith "v5 guarded edge changed disclosure kind."
        | value -> failwithf "Unknown replay mutation: %s" value

    let evaluate () =
        let package = fullPackage ()
        let encoded = canonicalPackageBytes ()
        require (Replay.CurrentFormatVersion = 6) "Weapon posture and fairness cursors must use replay format v6."

        let retainedV3 = retainedV3Bytes ()
        require (retainedV3.Length = 4_450) "The retained predecessor replay v3 fixture changed length."
        let retainedV3Digest = retainedV3 |> CanonicalHash.sha256 |> NumericFixtures.hex
        require
            (retainedV3Digest = "f820f32f86765f6be867caf7e91e5ab54ad67a68f30e85ea0cfce2dbaf44b88d")
            "The retained predecessor replay v3 fixture changed digest."
        let decodedV3 =
            Replay.decode Replay.defaultLimits retainedV3
            |> Result.defaultWith (fun error -> failwithf "Retained predecessor replay v3 did not decode: %A" error)
        require (decodedV3.FormatVersion = Replay.RulesArchiveFormatVersion) "The retained predecessor package is no longer replay v3."
        require (Replay.encode decodedV3 = retainedV3) "Retained predecessor replay v3 did not re-encode byte-exactly."
        match decodedV3.Content with
        | AuthorizedFullReplay full ->
            full.InitialSnapshot.Units
            |> Map.iter (fun _ unit ->
                require
                    (unit.Armor = { FrontRating = 0; RearRating = 0; Integrity = 0 }
                     && unit.Wounds.IsEmpty
                     && not unit.Incapacitated
                     && unit.Suppression = 0)
                    "Retained replay v3 did not apply deterministic combat defaults.")
            require full.InitialSnapshot.Board.Covers.IsEmpty "Retained replay v3 did not default covers deterministically."
        | PerspectivePlayback _ -> failwith "Retained full replay v3 changed disclosure kind."
        match Replay.runKernelReplay Replay.defaultLimits (Array.create 32 1uy) decodedV3 with
        | Ok(BrowserKernelVerified finalResult) ->
            require (finalResult.Tick = 0) "Retained zero-tick replay v3 changed its final tick."
        | result -> failwithf "Retained predecessor replay v3 did not verify: %A" result

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
            |> Result.defaultWith (fun error -> failwithf "Replay v4 combat snapshot did not decode: %A" error)
        require (Replay.encode decodedCombat = combatBytes) "Replay v4 combat snapshot did not round-trip byte-exactly."
        match decodedCombat.Content with
        | AuthorizedFullReplay full ->
            let expectedLegacy =
                let state = combatSnapshot ()
                { state with
                    Board =
                        { state.Board with
                            Edges =
                                state.Board.Edges
                                |> List.map (fun edge -> { edge with EdgeId = "legacy-semantic-edge"; SpatialRevision = 0 }) } }
            let expectedSnapshotBytes = Replay.snapshotBytes expectedLegacy
            require (Replay.snapshotBytes full.InitialSnapshot = expectedSnapshotBytes) "Replay v4 did not retain the complete combat snapshot."
            require (Replay.snapshotBytes full.Checkpoints.Head.State = expectedSnapshotBytes) "Replay v4 seek point lost combat state."
            let retained = full.InitialSnapshot.Units[Simulation.unitId 10]
            require (retained.Armor = { FrontRating = 61; RearRating = 23; Integrity = 74 }) "Replay v4 lost armor state."
            require (retained.Wounds.Length = 2 && retained.Wounds.Head.AttackId = "replay-v4-serious") "Replay v4 lost wound state."
            require (retained.Incapacitated && retained.Suppression = 37) "Replay v4 lost incapacity or suppression state."
            require (full.InitialSnapshot.Board.Covers["stone-wall"].Integrity = 63) "Replay v4 lost cover state."
        | PerspectivePlayback _ -> failwith "Replay v4 combat snapshot changed disclosure kind."
        match Replay.runKernelReplay Replay.defaultLimits engineHash decodedCombat with
        | Ok(BrowserKernelVerified result) ->
            require
                (result.StateHash = Replay.stateHashForFormatVersion Replay.PhysicalCombatFormatVersion (combatSnapshot ()))
                "Replay v4 combat seek verification returned the wrong hash."
        | result -> failwithf "Replay v4 combat seek verification failed: %A" result

        match decodedCombat.Content with
        | AuthorizedFullReplay full ->
            require full.InitialSnapshot.Awareness.IsEmpty "Replay v4 did not apply empty awareness defaults."
            require full.InitialSnapshot.Engagements.IsEmpty "Replay v4 did not apply empty engagement defaults."
        | PerspectivePlayback _ -> failwith "Replay v4 combat snapshot changed disclosure kind."

        let awarenessPackage = awarenessSnapshotPackageFor Replay.AwarenessFormatVersion
        let awarenessBytes = Replay.encode awarenessPackage
        let decodedAwareness =
            Replay.decode Replay.defaultLimits awarenessBytes
            |> Result.defaultWith (fun error -> failwithf "Replay v5 awareness snapshot did not decode: %A" error)
        require (Replay.encode decodedAwareness = awarenessBytes) "Replay v5 awareness snapshot did not round-trip byte-exactly."
        match decodedAwareness.Content with
        | AuthorizedFullReplay full ->
            require (full.InitialSnapshot.Awareness = (awarenessSnapshot ()).Awareness) "Replay v5 lost awareness state."
            let expectedLegacyEngagements =
                (awarenessSnapshot ()).Engagements
                |> Map.map (fun _ engagement ->
                    match engagement.Target with
                    | EngagementTarget.GuardedEdge(_, _, edge) ->
                        { engagement with Target = EngagementTarget.GuardedEdge("legacy-guarded-edge", 0, edge) }
                    | _ -> engagement)
            require (full.InitialSnapshot.Engagements = expectedLegacyEngagements) "Replay v5 lost legacy guarded-edge engagement state."
            require (full.OrderedInputs.Length = 2) "Replay v5 lost awareness/reaction inputs."
        | PerspectivePlayback _ -> failwith "Replay v5 awareness snapshot changed disclosure kind."

        let currentAwarenessPackage = awarenessSnapshotPackage ()
        let currentAwarenessBytes = Replay.encode currentAwarenessPackage
        let currentAwarenessDecoded =
            Replay.decode Replay.defaultLimits currentAwarenessBytes
            |> Result.defaultWith (fun error -> failwithf "Replay v6 awareness snapshot did not decode: %A" error)
        require (Replay.encode currentAwarenessDecoded = currentAwarenessBytes) "Replay v6 awareness snapshot did not round-trip byte-exactly."
        match currentAwarenessDecoded.Content with
        | AuthorizedFullReplay full ->
            require (full.InitialSnapshot.AwarenessCursor = (awarenessSnapshot ()).AwarenessCursor) "Replay v6 lost the fairness cursor."
            require (full.OrderedInputs.Length = 5) "Replay v6 lost posture, unit-target, or guarded-edge inputs."
            require
                (full.InitialSnapshot.Units[Simulation.unitId 10].WeaponPosture = (awarenessSnapshot ()).Units[Simulation.unitId 10].WeaponPosture)
                "Replay v6 lost weapon posture."
        | PerspectivePlayback _ -> failwith "Replay v6 awareness snapshot changed disclosure kind."

        let noAwarenessLimit = { Replay.defaultLimits with MaxAwarenessContacts = 0 }
        expectError
            (function MalformedPackage detail when detail.Contains "Resource limit exceeded for awareness contacts" -> true | _ -> false)
            (Replay.decode noAwarenessLimit awarenessBytes)
            "Replay v5 decoded awareness contacts beyond its resource limit."
        let noEngagementLimit = { Replay.defaultLimits with MaxEngagements = 0 }
        expectError
            (function MalformedPackage detail when detail.Contains "Resource limit exceeded for engagements" -> true | _ -> false)
            (Replay.decode noEngagementLimit awarenessBytes)
            "Replay v5 decoded engagements beyond its resource limit."

        let noCoverLimit = { Replay.defaultLimits with MaxCovers = 0 }
        expectError
            (function ResourceLimitExceeded("covers", 1, 0) -> true | _ -> false)
            (Replay.runKernelReplay noCoverLimit engineHash decodedCombat)
            "Replay v4 ignored its retained-cover resource limit."
        let oneWoundLimit = { Replay.defaultLimits with MaxWoundsPerUnit = 1 }
        expectError
            (function ResourceLimitExceeded("wounds per unit", 2, 1) -> true | _ -> false)
            (Replay.runKernelReplay oneWoundLimit engineHash decodedCombat)
            "Replay v4 ignored its retained-wound resource limit."
        expectError
            (function MalformedPackage detail when detail.Contains "Resource limit exceeded for covers" -> true | _ -> false)
            (Replay.decode noCoverLimit combatBytes)
            "Replay v4 decoded covers beyond its resource limit."
        expectError
            (function MalformedPackage detail when detail.Contains "Resource limit exceeded for wounds per unit" -> true | _ -> false)
            (Replay.decode oneWoundLimit combatBytes)
            "Replay v4 decoded wounds beyond its per-unit resource limit."

        let originalCombatHash = Replay.stateHash (combatSnapshot ())
        let requireHashMutation label (change: SimulationState -> SimulationState) =
            let changed = change (combatSnapshot ())
            require (Replay.stateHash changed <> originalCombatHash) ("Replay v4 state hash ignored " + label + ".")
        let redId = Simulation.unitId 10
        let changeRed (change: UnitState -> UnitState) (state: SimulationState) =
            let red = state.Units[redId]
            { state with Units = state.Units |> Map.add redId (change red) }
        requireHashMutation "armor" (changeRed (fun unit -> { unit with Armor = { unit.Armor with Integrity = unit.Armor.Integrity - 1 } }))
        requireHashMutation "wounds" (changeRed (fun unit -> { unit with Wounds = unit.Wounds.Tail }))
        requireHashMutation "incapacitation" (changeRed (fun unit -> { unit with Incapacitated = not unit.Incapacitated }))
        requireHashMutation "suppression" (changeRed (fun unit -> { unit with Suppression = unit.Suppression + 1 }))
        requireHashMutation "covers" (fun state -> { state with Board = { state.Board with Covers = Map.empty } })
        let originalAwarenessHash = Replay.stateHash (awarenessSnapshot ())
        let requireAwarenessHashMutation label change =
            require
                (Replay.stateHash (change (awarenessSnapshot ())) <> originalAwarenessHash)
                ("Replay v5 state hash ignored " + label + ".")
        requireAwarenessHashMutation "awareness contacts" (fun state -> { state with Awareness = Map.empty })
        requireAwarenessHashMutation "engagements" (fun state -> { state with Engagements = Map.empty })

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
            "Replay v4 accepted a combat-mutated seek snapshot under its retained hash."

        let physicalCodecPackage =
            package
            |> mapFull (fun full ->
                { full with
                    OrderedInputs =
                        [ input 1 1 (PhysicalAttack(Simulation.unitId 10, { Col = 2; Row = 0 }, WeaponProfile.AntiArmor)) ] })
        let physicalCodecBytes = Replay.encode physicalCodecPackage
        let physicalCodecDecoded =
            Replay.decode Replay.defaultLimits physicalCodecBytes
            |> Result.defaultWith (fun error -> failwithf "Replay v4 physical input did not decode: %A" error)
        require (Replay.encode physicalCodecDecoded = physicalCodecBytes) "Replay v4 physical input did not round-trip byte-exactly."

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
