namespace SIR.Server

open System
open System.Text.Json
open FS.GG.Game.Core
open SIR.Domain
open SIR.Match
open SIR.Simulation

type PhysicalCombatRequestDto =
    { AttackId: string
      Weapon: string }

type PhysicalCombatCellDto =
    { Column: int32
      Row: int32 }

type PhysicalCombatFactDto =
    { Step: string
      Subject: string
      Detail: string }

type PhysicalCombatResponseDto =
    { AttackId: string
      Profile: string
      BaseDamage: int32
      Penetration: int32
      Trace: PhysicalCombatCellDto array
      Cover: string
      Armor: string
      RemainingHealth: int32
      Wounds: string array
      Suppression: int32
      Incapacitated: bool
      Facts: PhysicalCombatFactDto array
      CanonicalByteCount: int32 }

[<RequireQualifiedAccess>]
module PhysicalCombatDiagnostics =
    let private options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
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
        let cover = { CoverId = "roadblock-2"; Cell = cell 2 0; Integrity = 100; ProjectileBlocking = false }
        let identity = SpatialAuthorityIdentity.create "physical-combat-drill" "combat-rules-v1" 1L "player-authority" 1L |> Result.defaultWith failwith
        { Spatial =
            { Identity = identity; Minimum = cell 0 0; Maximum = cell 6 2; Terrain = Map.empty
              Boundaries = []; Occupancy = Map.empty; DisclosedRevisionTokens = Set.empty }
          Combatants = [ attacker.EntityId, attacker; target.EntityId, target ] |> Map.ofList
          Covers = [ cover.CoverId, cover ] |> Map.ofList }

    let evaluate (json: string) =
        try
            let boxedParsed = JsonSerializer.Deserialize<PhysicalCombatRequestDto>(json, options) |> box
            if isNull boxedParsed then
                Error "invalid physical combat identity"
            else
                let parsed = unbox<PhysicalCombatRequestDto> boxedParsed
                let profile =
                    if String.IsNullOrWhiteSpace parsed.AttackId || parsed.AttackId.Length > 96 then None
                    else
                        match parsed.Weapon with
                        | "AntiArmor" -> Some WeaponProfile.AntiArmor
                        | "Rifle" -> Some WeaponProfile.Rifle
                        | "SupportWeapon" -> Some WeaponProfile.SupportWeapon
                        | "LobbedArea" -> Some WeaponProfile.LobbedArea
                        | _ -> None
                match profile with
                | None -> Error "invalid physical combat weapon"
                | Some weapon ->
                    let request = { AttackId = parsed.AttackId; AttackerId = "red-10"; AimCell = cell 4 0; Weapon = weapon; Limits = Combat.defaultLimits }
                    match PhysicalCombatServices.resolve (drillWorld ()) request with
                    | Error rejection -> Error(sprintf "physical combat rejected: %A" rejection)
                    | Ok service ->
                        let target = service.Result.World.Combatants["blue-20"]
                        let trace = service.Result.Facts |> List.tryPick (function CombatFact.TraceEvaluated(cells, _, _) -> Some cells | _ -> None) |> Option.defaultValue []
                        let cover =
                            service.Result.Facts
                            |> List.tryPick (function
                                | CombatFact.CoverResolved(_, source, retained) ->
                                    let coverSource = Option.defaultValue "none" source
                                    Some($"{coverSource} · retained {retained}" + "%")
                                | _ -> None)
                            |> Option.defaultValue "none"
                        let armor = service.Result.Facts |> List.tryPick (function CombatFact.ArmorResolved(_, arc, rating, penetration, retained) -> Some($"{arc} rating {rating} vs penetration {penetration} · retained {retained}" + "%") | _ -> None) |> Option.defaultValue "none"
                        let response =
                            { AttackId = service.Result.Request.AttackId; Profile = string service.Result.Parameters.Profile
                              BaseDamage = service.Result.Parameters.BaseDamage; Penetration = service.Result.Parameters.Penetration
                              Trace = trace |> List.map cellDto |> List.toArray; Cover = cover; Armor = armor
                              RemainingHealth = target.Health; Wounds = target.Wounds |> List.map (fun wound -> string wound.Severity) |> List.toArray
                              Suppression = target.Suppression; Incapacitated = target.Incapacitated
                              Facts = service.Result.Facts |> List.map factDto |> List.toArray
                              CanonicalByteCount = service.CanonicalBytes.Length }
                        Ok(JsonSerializer.Serialize(response, options))
        with :? JsonException -> Error "invalid physical combat JSON"
