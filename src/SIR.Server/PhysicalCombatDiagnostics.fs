namespace SIR.Server

open System
open System.Text.Json
open FS.GG.Game.Core
open SIR.Domain
open SIR.Match
open SIR.Simulation

type PhysicalCombatRequestDto =
    { AttackId: string
      Scenario: string }

type PhysicalCombatCellDto =
    { Column: int32
      Row: int32 }

type PhysicalCombatFactDto =
    { Step: string
      Subject: string
      Detail: string }

type PhysicalCombatProfileDto =
    { AttackId: string
      Profile: string
      BaseDamage: int32
      Penetration: int32
      Trace: PhysicalCombatCellDto array
      CoverSource: string
      CoverRetainedPercent: int32
      ArmorArc: string
      ArmorRating: int32
      ArmorRetainedPercent: int32
      RemainingHealth: int32
      Wounds: string array
      Suppression: int32
      Incapacitated: bool
      CoverIntegrityBefore: int32
      CoverIntegrityAfter: int32
      CoverDestroyed: bool
      Facts: PhysicalCombatFactDto array
      CanonicalByteCount: int32 }

type PhysicalCombatReplayDto =
    { FormatVersion: int32
      Verified: bool
      SeekPointsVerified: int32
      FinalTick: int32
      FinalStateHash: string }

type PhysicalCombatResponseDto =
    { Scenario: string
      InitialCoverIntegrity: int32
      FinalCoverIntegrity: int32
      CoverDestroyed: bool
      Profiles: PhysicalCombatProfileDto array
      Replay: PhysicalCombatReplayDto }

[<RequireQualifiedAccess>]
module PhysicalCombatDiagnostics =
    let private options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
    let private scenarioId = "four-profile-cover-replay-v1"
    let private coverId = "roadblock-2"
    let private profiles = [ WeaponProfile.Rifle; WeaponProfile.SupportWeapon; WeaponProfile.AntiArmor; WeaponProfile.LobbedArea ]
    let private cell col row: Cell = { Col = col; Row = row }
    let private cellDto value = { Column = value.Col; Row = value.Row }

    let private factDto = function
        | CombatFact.Eligible attacker -> { Step = "Eligibility"; Subject = attacker; Detail = "eligible" }
        | CombatFact.Committed(profile, preparation) -> { Step = "Preparation"; Subject = string profile; Detail = $"commitment={preparation}" }
        | CombatFact.TraceEvaluated(cells, edges, _) -> { Step = "Physical trace"; Subject = "projectile"; Detail = $"cells={cells.Length};edges={edges.Length}" }
        | CombatFact.Contact(entity, at, index) -> { Step = "Collision"; Subject = entity; Detail = $"cell=({at.Col},{at.Row});index={index}" }
        | CombatFact.CoverResolved(entity, cover, retained) ->
            let source = Option.defaultValue "none" cover
            { Step = "Cover"; Subject = entity; Detail = $"source={source};retained={retained}" + "%" }
        | CombatFact.ArmorResolved(entity, arc, rating, penetration, retained) -> { Step = "Armor"; Subject = entity; Detail = $"arc={arc};rating={rating};penetration={penetration};retained={retained}" + "%" }
        | CombatFact.HealthChanged(entity, damage, remaining) -> { Step = "HP"; Subject = entity; Detail = $"damage={damage};remaining={remaining}" }
        | CombatFact.WoundApplied(entity, severity) -> { Step = "Wound"; Subject = entity; Detail = string severity }
        | CombatFact.Incapacitated entity -> { Step = "Incapacitated"; Subject = entity; Detail = "true" }
        | CombatFact.SuppressionChanged(entity, delta, total) -> { Step = "Suppression"; Subject = entity; Detail = $"delta={delta};total={total}" }
        | CombatFact.CoverDamaged(cover, damage, remaining) -> { Step = "Environment"; Subject = cover; Detail = $"damage={damage};remaining={remaining}" }
        | CombatFact.CoverDestroyed cover -> { Step = "Environment"; Subject = cover; Detail = "destroyed" }

    let private drillWorld () =
        let attacker =
            { EntityId = "red-10"; Faction = "red"; Cell = cell 0 0; Facing = Direction8.East; Health = 100
              Armor = { FrontRating = 0; RearRating = 0; Integrity = 0 }; Wounds = []; Incapacitated = false; Suppression = 0 }
        let target =
            { EntityId = "blue-20"; Faction = "blue"; Cell = cell 4 0; Facing = Direction8.West; Health = 100
              Armor = { FrontRating = 50; RearRating = 20; Integrity = 100 }; Wounds = []; Incapacitated = false; Suppression = 0 }
        let cover = { CoverId = coverId; Cell = cell 2 0; Integrity = 50; ProjectileBlocking = false }
        let identity = SpatialAuthorityIdentity.create "physical-combat-drill" "combat-rules-v1" 1L "player-authority" 1L |> Result.defaultWith failwith
        { Spatial =
            { Identity = identity; Minimum = cell 0 0; Maximum = cell 6 2; Terrain = Map.empty
              Boundaries = []; Occupancy = Map.empty; DisclosedRevisionTokens = Set.empty }
          Combatants = [ attacker.EntityId, attacker; target.EntityId, target ] |> Map.ofList
          Covers = [ cover.CoverId, cover ] |> Map.ofList }

    let private coverIntegrity (world: CombatWorld) =
        world.Covers |> Map.tryFind coverId |> Option.map _.Integrity |> Option.defaultValue 0

    let private projectProfile beforeIntegrity (service: PhysicalCombatServiceResponse) =
        let result = service.Result
        let target = result.World.Combatants["blue-20"]
        let trace = result.Facts |> List.tryPick (function CombatFact.TraceEvaluated(cells, _, _) -> Some cells | _ -> None) |> Option.defaultValue []
        let coverSource, coverRetained =
            result.Facts
            |> List.tryPick (function
                | CombatFact.CoverResolved(_, source, retained) ->
                    Some(Option.defaultValue "none" source, retained)
                | _ -> None)
            |> Option.defaultValue ("none", 100)
        let armorArc, armorRating, armorRetained =
            result.Facts
            |> List.tryPick (function
                | CombatFact.ArmorResolved(_, arc, rating, _, retained) -> Some(string arc, rating, retained)
                | _ -> None)
            |> Option.defaultValue ("none", 0, 100)
        let afterIntegrity = coverIntegrity result.World
        { AttackId = result.Request.AttackId
          Profile = string result.Parameters.Profile
          BaseDamage = result.Parameters.BaseDamage
          Penetration = result.Parameters.Penetration
          Trace = trace |> List.map cellDto |> List.toArray
          CoverSource = coverSource
          CoverRetainedPercent = coverRetained
          ArmorArc = armorArc
          ArmorRating = armorRating
          ArmorRetainedPercent = armorRetained
          RemainingHealth = target.Health
          Wounds = target.Wounds |> List.map (fun wound -> string wound.Severity) |> List.toArray
          Suppression = target.Suppression
          Incapacitated = target.Incapacitated
          CoverIntegrityBefore = beforeIntegrity
          CoverIntegrityAfter = afterIntegrity
          CoverDestroyed = beforeIntegrity > 0 && afterIntegrity = 0
          Facts = result.Facts |> List.map factDto |> List.toArray
          CanonicalByteCount = service.CanonicalBytes.Length }

    let private bounded value =
        BoundedInt32.create 0 100 value |> Result.defaultWith (string >> failwith)

    let private replayInitialState () =
        let red =
            { Id = Simulation.unitId 10; Side = Side.Red; Cell = cell 0 0; Health = bounded 100
              Armor = { FrontRating = 0; RearRating = 0; Integrity = 0 }; Wounds = []; Incapacitated = false
              Suppression = 0; BodyFacing = Direction8.East; AttentionDirection = Direction8.East; WeaponPosture = WeaponPosture.Mobile }
        let blue =
            { Id = Simulation.unitId 20; Side = Side.Blue; Cell = cell 4 0; Health = bounded 100
              Armor = { FrontRating = 50; RearRating = 20; Integrity = 100 }; Wounds = []; Incapacitated = false
              Suppression = 0; BodyFacing = Direction8.West; AttentionDirection = Direction8.West; WeaponPosture = WeaponPosture.Mobile }
        { Tick = 0
          Board =
            { Minimum = cell 0 0; Maximum = cell 6 2; Edges = []
              Covers = [ coverId, { CoverId = coverId; Cell = cell 2 0; Integrity = 50; ProjectileBlocking = false } ] |> Map.ofList }
          Units = [ red.Id, red; blue.Id, blue ] |> Map.ofList
          Observations = Set.empty
          Awareness = Map.empty
          Engagements = Map.empty
          AwarenessCursor = 0 }

    let private replayEvidence () =
        let initial = replayInitialState ()
        let inputs : ReplayInput list =
            profiles
            |> List.mapi (fun index profile ->
                { Tick = index + 1; Sequence = index + 1
                  Input = KernelInput.PhysicalAttack(Simulation.unitId 10, cell 4 0, profile) })
        let checkpoint tick state events =
            { Tick = tick; State = state; StateHash = Replay.stateHash state; EventHash = Replay.eventHash events }
        let mutable state = initial
        let mutable finalEvents = []
        let mutable checkpoints = [ checkpoint 0 initial [] ]
        for tick in 1 .. profiles.Length do
            let journal = inputs |> List.filter (fun input -> input.Tick = tick) |> List.map _.Input
            let result = Simulation.runTick state journal
            state <- result.State
            finalEvents <- result.Events
            if tick < profiles.Length then checkpoints <- checkpoints @ [ checkpoint tick state finalEvents ]
        let finalResult =
            { Tick = state.Tick; OutcomeCode = 1; StateHash = Replay.stateHash state; EventHash = Replay.eventHash finalEvents }
        let engineHash = [| for value in 1 .. 32 -> byte value |]
        let package =
            { FormatVersion = int32 Replay.CurrentFormatVersion
              EngineHash = engineHash
              RulesetHash = CombatRules.packageIdentity.ManifestDigest
              FullReplayAuthorized = true
              RulesArchive = Some(Replay.createRulesArchive CombatRules.packageIdentity CombatRules.registry [])
              Content =
                ReplayContent.AuthorizedFullReplay
                    { InitialSnapshot = initial; OrderedInputs = inputs; AcceptedWasmOutputs = []
                      Checkpoints = checkpoints; FinalResult = finalResult } }
        match Replay.runKernelReplay Replay.defaultLimits engineHash package with
        | Ok(ReplayVerification.BrowserKernelVerified verified) ->
            { FormatVersion = package.FormatVersion
              Verified = true
              SeekPointsVerified = checkpoints.Length
              FinalTick = verified.Tick
              FinalStateHash = Convert.ToHexString(verified.StateHash).ToLowerInvariant() }
        | Ok result -> failwithf "unexpected physical replay verification: %A" result
        | Error error -> failwithf "physical replay verification failed: %A" error

    let evaluate (json: string) =
        try
            let boxedParsed = JsonSerializer.Deserialize<PhysicalCombatRequestDto>(json, options) |> box
            if isNull boxedParsed then Error "invalid physical combat identity" else
            let parsed = unbox<PhysicalCombatRequestDto> boxedParsed
            if String.IsNullOrWhiteSpace parsed.AttackId || parsed.AttackId.Length > 96 then
                Error "invalid physical combat identity"
            elif parsed.Scenario <> scenarioId then
                Error "invalid physical combat scenario"
            else
                let initialWorld = drillWorld ()
                let resolved =
                    ((Ok(initialWorld, [])), profiles)
                    ||> List.fold (fun state profile ->
                        match state with
                        | Error error -> Error error
                        | Ok(world, outcomes) ->
                            let beforeIntegrity = coverIntegrity world
                            let request =
                                { AttackId = $"{parsed.AttackId}-{string profile}"
                                  AttackerId = "red-10"; AimCell = cell 4 0; Weapon = profile; Limits = Combat.defaultLimits }
                            match PhysicalCombatServices.resolve world request with
                            | Error rejection -> Error(sprintf "physical combat rejected: %A" rejection)
                            | Ok service -> Ok(service.Result.World, outcomes @ [ projectProfile beforeIntegrity service ]))
                match resolved with
                | Error error -> Error error
                | Ok(finalWorld, outcomes) ->
                    let response =
                        { Scenario = scenarioId
                          InitialCoverIntegrity = coverIntegrity initialWorld
                          FinalCoverIntegrity = coverIntegrity finalWorld
                          CoverDestroyed = not (finalWorld.Covers.ContainsKey coverId)
                          Profiles = List.toArray outcomes
                          Replay = replayEvidence () }
                    Ok(JsonSerializer.Serialize(response, options))
        with :? JsonException -> Error "invalid physical combat JSON"
