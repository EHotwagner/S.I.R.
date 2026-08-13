namespace SIR.Conformance

open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

[<RequireQualifiedAccess>]
module CombatFixtures =
    let private require condition message = if not condition then failwith message
    let private cell col row: Cell = { Col = col; Row = row }
    let private identity revision = SpatialAuthorityIdentity.create "combat-fixture" "combat-rules-v1" revision "full-authority" revision |> Result.defaultWith failwith
    let private spatial revision maximum =
        { Identity = identity revision; Minimum = cell 0 0; Maximum = maximum; Terrain = Map.empty; Boundaries = []; Occupancy = Map.empty; DisclosedRevisionTokens = Set.empty }
    let private armor facing = { FrontRating = 50; RearRating = 20; Integrity = 100 }, facing
    let private unit id faction position facing health =
        let armorState, facing = armor facing
        { EntityId = id; Faction = faction; Cell = position; Facing = facing; Health = health; Armor = armorState; Wounds = []; Incapacitated = false; Suppression = 0 }
    let private unarmored id faction position facing health = { unit id faction position facing health with Armor = { FrontRating = 0; RearRating = 0; Integrity = 0 } }
    let private world revision units covers maximum = { Spatial = spatial revision maximum; Combatants = units |> List.map (fun unit -> unit.EntityId, unit) |> Map.ofList; Covers = covers |> List.map (fun cover -> cover.CoverId, cover) |> Map.ofList }
    let private request id profile aim = { AttackId = id; AttackerId = "attacker"; AimCell = aim; Weapon = profile; Limits = Combat.defaultLimits }
    let private resolved state attack = Combat.resolve state attack |> Result.defaultWith (fun error -> failwithf "Combat fixture rejected: %A" error)
    let private factsOf picker result = result.Facts |> List.choose picker
    let private health id result = result.World.Combatants[id].Health

    let evaluate injectDivergence =
        let attacker = unarmored "attacker" "red" (cell 0 0) East 100
        let openTarget = unarmored "open" "blue" (cell 4 0) West 100
        let openResult = resolved (world 1L [ attacker; openTarget ] [] (cell 12 12)) (request "open-rifle" WeaponProfile.Rifle openTarget.Cell)
        require (health "open" openResult = 75) "Open rifle damage changed."
        require (openResult.Facts |> List.exists (function CombatFact.WoundApplied("open", WoundSeverity.Serious) -> true | _ -> false)) "A 25-damage hit did not create a serious wound."
        let factIndex predicate = openResult.Facts |> List.findIndex predicate
        let coverIndex = factIndex (function CombatFact.CoverResolved _ -> true | _ -> false)
        let armorIndex = factIndex (function CombatFact.ArmorResolved _ -> true | _ -> false)
        let healthIndex = factIndex (function CombatFact.HealthChanged _ -> true | _ -> false)
        let suppressionIndex = factIndex (function CombatFact.SuppressionChanged _ -> true | _ -> false)
        require (coverIndex < armorIndex && armorIndex < healthIndex && healthIndex < suppressionIndex) "Combat consequence facts lost canonical cover/armor/health/suppression ordering."
        require (openResult.RuleApplications.Length = 1 && openResult.RuleApplications.Head.PackageManifestDigest = CombatRules.packageIdentity.ManifestDigest) "Combat result was not bound to the executable rules identity."
        let committedDamage =
            match openResult.RuleApplications.Head.Outcome.Value with
            | RuleValue.IntegerValue value -> value
            | value -> failwithf "Combat transition returned a non-integer damage outcome: %A" value
        require (committedDamage = 25 && health "open" openResult = 100 - committedDamage) "World damage diverged from the authoritative executable rule outcome."

        let unreachableSpatial =
            let edge = Edges.edgeBetween (cell 1 0) (cell 2 0) |> Option.defaultWith (fun () -> failwith "fixture cells were not adjacent")
            { spatial 101L (cell 12 12) with
                Boundaries =
                    [ { Edge = edge
                        Permeability = { Ground = true; Vision = true; Projectile = false }
                        RevisionToken = "direct-unreachable" } ] }
        let unreachableWorld = { Combatants = [ attacker; openTarget ] |> List.map (fun unit -> unit.EntityId, unit) |> Map.ofList; Covers = Map.empty; Spatial = unreachableSpatial }
        let unreachableResult = resolved unreachableWorld (request "direct-unreachable" WeaponProfile.Rifle openTarget.Cell)
        require (health "open" unreachableResult = 100) "An unreachable direct trace committed damage."
        require (unreachableResult.RuleApplications.IsEmpty) "An unreachable direct trace emitted a committed target rule application."
        require (unreachableResult.Facts |> List.exists (function CombatFact.HealthChanged _ -> true | _ -> false) |> not) "An unreachable direct trace emitted a health mutation fact."
        let zeroRuleResult =
            CombatRules.resolveConsequences 100 0 10
                { Attacker = attacker.Cell
                  TargetFootprint = [ openTarget.Cell ]
                  VisibleSamples = 0
                  TotalSamples = 1
                  RangeCells = 4
                  Suppression = FixedPoint.zero
                  BaseDamage = FixedPoint.fromRatio 25 1 |> Result.defaultWith (fun _ -> failwith "invalid fixture damage")
                  ArmorRetention = FixedPoint.fromRatio 1 1 |> Result.defaultWith (fun _ -> failwith "invalid fixture retention")
                  EventId = "direct-rule-zero" }
            |> Result.defaultWith failwith
        require (zeroRuleResult.Damage = 0 && zeroRuleResult.RemainingHealth = 100 && zeroRuleResult.SuppressionDelta = 0) "A rule-zero direct attack committed consequences."

        let partial = { CoverId = "partial"; Cell = cell 2 0; Integrity = 100; ProjectileBlocking = false }
        let partialResult = resolved (world 2L [ attacker; openTarget ] [ partial ] (cell 12 12)) (request "partial-cover" WeaponProfile.Rifle openTarget.Cell)
        require (health "open" partialResult = 87) "Partial cover did not halve retained rifle damage."
        require (partialResult.Facts |> List.exists (function CombatFact.CoverResolved("open", Some "partial", 50) -> true | _ -> false)) "Partial cover decision was not emitted."

        let full = { partial with CoverId = "full"; Integrity = 100; ProjectileBlocking = true }
        let fullResult = resolved (world 3L [ attacker; openTarget ] [ full ] (cell 12 12)) (request "full-cover" WeaponProfile.Rifle openTarget.Cell)
        require (health "open" fullResult = 100) "Full cover failed to stop the direct trace."
        require (Map.tryFind full.Cell fullResult.World.Spatial.Terrain = Some SpatialTerrain.Blocked) "Surviving full cover was not projected into projectile permeability."

        let supportCoverResult = resolved (world 31L [ attacker; openTarget ] [ { full with Cell = openTarget.Cell } ] (cell 12 12)) (request "support-full-cover" WeaponProfile.SupportWeapon openTarget.Cell)
        require (health "open" supportCoverResult = 85) "Support-area impact failed to apply through the full-cover cell."
        require (supportCoverResult.World.Covers["full"].Integrity = 93) "Support-area impact did not commit the cover rule result."
        require (supportCoverResult.RuleApplications |> List.exists (fun application -> RuleId.value application.RuleId = "COMBAT-COVER-DESTRUCTION-001")) "Cover impact was absent from executable rule applications."

        let front = unit "armored" "blue" (cell 4 0) West 100
        let rear = { front with Facing = East }
        let frontResult = resolved (world 4L [ attacker; front ] [] (cell 12 12)) (request "front-armor" WeaponProfile.Rifle front.Cell)
        let rearResult = resolved (world 5L [ attacker; rear ] [] (cell 12 12)) (request "rear-armor" WeaponProfile.Rifle rear.Cell)
        require (health "armored" frontResult = 90 && health "armored" rearResult = 75) "Directional armor or penetration changed."
        let antiResult = resolved (world 6L [ attacker; front ] [] (cell 12 12)) (request "anti-armor" WeaponProfile.AntiArmor front.Cell)
        require (health "armored" antiResult = 50) "Anti-armor penetration failed to retain full damage."

        let intervening = unarmored "civilian" "civilian" (cell 2 0) North 100
        let interveningResult = resolved (world 7L [ attacker; intervening; openTarget ] [] (cell 12 12)) (request "intervening" WeaponProfile.Rifle openTarget.Cell)
        require (health "civilian" interveningResult = 75 && health "open" interveningResult = 100) "Intervening collision did not stop at the first unit."

        let friendly = unarmored "friendly" "red" (cell 4 1) North 100
        let enemy = unarmored "enemy" "blue" (cell 5 0) North 100
        let areaResult = resolved (world 8L [ attacker; friendly; enemy ] [] (cell 12 12)) (request "support-area" WeaponProfile.SupportWeapon (cell 4 0))
        require (health "friendly" areaResult < 100 && areaResult.World.Combatants["friendly"].Suppression = 25) "Friendly area recipient received implicit immunity or missed suppression."
        require (health "enemy" areaResult < 100 && areaResult.World.Combatants["enemy"].Suppression = 25) "Enemy area recipient was not resolved."

        let lobTarget = unarmored "lob-target" "blue" (cell 6 6) North 40
        let lobResult = resolved (world 9L [ attacker; lobTarget ] [] (cell 12 12)) (request "lobbed-incapacity" WeaponProfile.LobbedArea lobTarget.Cell)
        require (health "lob-target" lobResult = 10 && lobResult.World.Combatants["lob-target"].Suppression = 30) "Lobbed area consequence changed."
        let secondLob = resolved lobResult.World (request "lobbed-incapacity-2" WeaponProfile.LobbedArea lobTarget.Cell)
        require (secondLob.World.Combatants["lob-target"].Incapacitated && health "lob-target" secondLob = 0) "Zero HP did not produce incapacity."

        let fragile = { CoverId = "fragile"; Cell = cell 2 0; Integrity = 10; ProjectileBlocking = true }
        let destruction = resolved (world 10L [ attacker; openTarget ] [ fragile ] (cell 12 12)) (request "cover-destruction" WeaponProfile.Rifle openTarget.Cell)
        require (not (Map.containsKey "fragile" destruction.World.Covers)) "Destroyed cover remained in combat state."
        require (not (Map.containsKey fragile.Cell destruction.World.Spatial.Terrain)) "Destroyed cover remained projectile-impermeable in the projected spatial world."
        require (destruction.Facts |> List.exists (function CombatFact.CoverDestroyed "fragile" -> true | _ -> false)) "Cover destruction fact was omitted."
        require (health "open" destruction = 100) "The projectile that destroyed full cover incorrectly continued through it in the same attack."
        let afterDestruction = resolved destruction.World (request "after-cover-destruction" WeaponProfile.Rifle openTarget.Cell)
        require (health "open" afterDestruction = 75) "Destroyed cover continued blocking a later trace."

        let recovered, recoveryFacts = Combat.recover areaResult.World
        require (recovered.Combatants["friendly"].Suppression = 20 && recoveryFacts |> List.exists (function CombatFact.SuppressionChanged("friendly", -5, 20) -> true | _ -> false)) "Suppression recovery was not a distinct deterministic transition."
        require (Combat.suppressionEffectivenessPercent 75 = 40 && Combat.suppressionTimingPercent 75 = 175) "Suppression bands changed."

        match Combat.resolve (world 11L [ attacker; openTarget ] [] (cell 12 12)) { request "bad-limits" WeaponProfile.Rifle openTarget.Cell with Limits = { Combat.defaultLimits with MaximumFacts = 4097 } } with
        | Error(CombatRejection.InvalidRequest _) -> ()
        | verdict -> failwithf "Out-of-schema limits were accepted: %A" verdict

        let canonical =
            [ openResult; unreachableResult; partialResult; fullResult; supportCoverResult; frontResult; rearResult; antiResult; interveningResult; areaResult; lobResult; secondLob; destruction; afterDestruction ]
            |> List.map Combat.canonicalResultBytes
            |> CanonicalEncoding.concatenate
        if injectDivergence then
            let changed = Array.copy canonical
            changed[0] <- changed[0] ^^^ 1uy
            changed
        else canonical

#if !FABLE_COMPILER
    let performanceWorkload () =
        let maximum = cell 31 31
        let combatants =
            [ for index in 0 .. 99 ->
                let position = cell (index % 10) (index / 10)
                unarmored (if index = 0 then "attacker" else "unit-" + string index) (if index % 2 = 0 then "red" else "blue") position East 100 ]
        let state = world 50L combatants [] maximum
        let attacks = [ for index in 1 .. 50 -> request ("stress-" + string index) WeaponProfile.SupportWeapon (cell (index % 10) ((index * 3) % 10)) ]
        let run () = attacks |> List.fold (fun current attack -> (resolved current attack).World) state
        run () |> ignore
        let stopwatch = System.Diagnostics.Stopwatch.StartNew()
        let final = run ()
        stopwatch.Stop()
        let representativeWatch = System.Diagnostics.Stopwatch.StartNew()
        let representative = evaluate false
        representativeWatch.Stop()
        final, representative.Length, representativeWatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds
#endif
