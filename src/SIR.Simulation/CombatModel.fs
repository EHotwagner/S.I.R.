namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

type DamageType = Ballistic | Explosive | AntiArmor
type WeaponProfile = Rifle | SupportWeapon | AntiArmor | LobbedArea
type WeaponParameters = { Profile: WeaponProfile; DamageType: DamageType; BaseDamage: int32; Penetration: int32; Suppression: int32; RangeCells: int32; AreaRadius: int32; Lobbed: bool }
type ArmorArc = Front | RearOrFlank
type ArmorState = { FrontRating: int32; RearRating: int32; Integrity: int32 }
type WoundSeverity = Serious | Critical
type Wound = { AttackId: string; Severity: WoundSeverity; Damage: int32 }
type CombatantState = { EntityId: string; Faction: string; Cell: Cell; Facing: Direction8; Health: int32; Armor: ArmorState; Wounds: Wound list; Incapacitated: bool; Suppression: int32 }
type CoverState = { CoverId: string; Cell: Cell; Integrity: int32; ProjectileBlocking: bool }
type CombatWorld = { Spatial: ProjectedSpatialWorld; Combatants: Map<string, CombatantState>; Covers: Map<string, CoverState> }
type CombatLimits = { MaximumTraceCells: int32; MaximumAreaCells: int32; MaximumRecipients: int32; MaximumFacts: int32; MaximumExplanationBytes: int32 }
type CombatRequest = { AttackId: string; AttackerId: string; AimCell: Cell; Weapon: WeaponProfile; Limits: CombatLimits }
type CombatRejection = InvalidRequest of string | Ineligible of string | OutOfRange of distance: int32 * maximum: int32 | SpatialUnavailable of SpatialOutcome | LimitExceeded of name: string * observed: int32 * maximum: int32
type CombatFact = Eligible of attackerId: string | Committed of profile: WeaponProfile * preparationRaw: int32 | TraceEvaluated of crossedCells: Cell list * crossedEdges: Edge list * spatialBytes: byte array | Contact of entityId: string * cell: Cell * traceIndex: int32 | CoverResolved of entityId: string * coverId: string option * retainedPercent: int32 | ArmorResolved of entityId: string * arc: ArmorArc * effectiveRating: int32 * penetration: int32 * retainedPercent: int32 | HealthChanged of entityId: string * damage: int32 * remainingHealth: int32 | WoundApplied of entityId: string * severity: WoundSeverity | Incapacitated of entityId: string | SuppressionChanged of entityId: string * delta: int32 * total: int32 | CoverDamaged of coverId: string * damage: int32 * remainingIntegrity: int32 | CoverDestroyed of coverId: string
type CombatResult = { SchemaVersion: int32; Request: CombatRequest; Parameters: WeaponParameters; World: CombatWorld; Facts: CombatFact list; SpatialEvidence: SpatialQueryResult option; RuleApplications: RuleApplication list }
type private CandidateSubject = CoverCandidate of CoverState | CombatantCandidate of CombatantState

[<RequireQualifiedAccess>]
module Combat =
    let schemaVersion = 1
    let compatibilityProfile = "sir-physical-combat-v1"
    let defaultLimits = { MaximumTraceCells = 256; MaximumAreaCells = 256; MaximumRecipients = 256; MaximumFacts = 4096; MaximumExplanationBytes = 65_536 }
    let parameters = function
        | WeaponProfile.Rifle -> { Profile = WeaponProfile.Rifle; DamageType = DamageType.Ballistic; BaseDamage = 25; Penetration = 20; Suppression = 10; RangeCells = 16; AreaRadius = 0; Lobbed = false }
        | WeaponProfile.SupportWeapon -> { Profile = WeaponProfile.SupportWeapon; DamageType = DamageType.Ballistic; BaseDamage = 15; Penetration = 10; Suppression = 25; RangeCells = 20; AreaRadius = 1; Lobbed = false }
        | WeaponProfile.AntiArmor -> { Profile = WeaponProfile.AntiArmor; DamageType = DamageType.AntiArmor; BaseDamage = 50; Penetration = 70; Suppression = 12; RangeCells = 12; AreaRadius = 0; Lobbed = false }
        | WeaponProfile.LobbedArea -> { Profile = WeaponProfile.LobbedArea; DamageType = DamageType.Explosive; BaseDamage = 30; Penetration = 25; Suppression = 30; RangeCells = 10; AreaRadius = 2; Lobbed = true }

    let private clamp (maximum: int32) (value: int32) = max 0 value |> min maximum
    let suppressionEffectivenessPercent (suppression: int32) = if suppression >= 75 then 40 elif suppression >= 50 then 60 elif suppression >= 25 then 80 else 100
    let suppressionTimingPercent (suppression: int32) = if suppression >= 75 then 175 elif suppression >= 50 then 150 elif suppression >= 25 then 125 else 100
    let private distance (left: Cell) (right: Cell) = max (abs (left.Col - right.Col)) (abs (left.Row - right.Row))
    let private inBounds (spatial: ProjectedSpatialWorld) (cell: Cell) = cell.Col >= spatial.Minimum.Col && cell.Col <= spatial.Maximum.Col && cell.Row >= spatial.Minimum.Row && cell.Row <= spatial.Maximum.Row
    let private validLimits (limits: CombatLimits) =
        limits.MaximumTraceCells > 0 && limits.MaximumTraceCells <= 256
        && limits.MaximumAreaCells > 0 && limits.MaximumAreaCells <= 256
        && limits.MaximumRecipients > 0 && limits.MaximumRecipients <= 256
        && limits.MaximumFacts > 0 && limits.MaximumFacts <= 4096
        && limits.MaximumExplanationBytes > 0 && limits.MaximumExplanationBytes <= 65_536
    let private profileFor (attacker: CombatantState) =
        { ProfileId = "combat-projectile-v1"; Modality = SpatialModality.ProjectileTrace; Stance = "standing"; HeightBand = 1; Facing = attacker.Facing }
    let private spatialRequest (attacker: CombatantState) (request: CombatRequest) =
        { QueryId = request.AttackId + ":trace"; QueryKind = SpatialQueryKind.LineTrace; Origin = attacker.Cell; Target = request.AimCell; Footprint = [ { Col = 0; Row = 0 } ]; Profile = profileFor attacker; Bounds = { SpatialQuery.defaultBounds with MaximumCrossedItems = request.Limits.MaximumTraceCells } }
    let private areaCells (spatial: ProjectedSpatialWorld) (center: Cell) (radius: int32) =
        [ for row in center.Row - radius .. center.Row + radius do
            for col in center.Col - radius .. center.Col + radius do
                let candidate = { Col = col; Row = row }
                let d = distance center candidate
                if d <= radius && inBounds spatial candidate then yield d, candidate ]
        |> List.sortBy (fun (d, cell) -> d, cell.Row, cell.Col)
        |> List.map snd
    let private fixedRatio (numerator: int32) (denominator: int32) = FixedPoint.fromRatio numerator denominator |> Result.defaultWith (fun _ -> FixedPoint.zero)
    let private directionToSource (source: Cell) (target: Cell) = Direction8.tryFromDelta (source.Col - target.Col) (source.Row - target.Row)
    let private armorResolution (source: Cell) (penetration: int32) (armor: ArmorState) (target: CombatantState) =
        let arc = if directionToSource source target.Cell = Some target.Facing then ArmorArc.Front else ArmorArc.RearOrFlank
        let rating = if arc = ArmorArc.Front then armor.FrontRating else armor.RearRating
        let effective = rating * clamp 100 armor.Integrity / 100
        let retained = if effective <= 0 || penetration >= effective then 100 else max 10 (penetration * 100 / effective)
        arc, effective, retained
    let private directEvidence (world: CombatWorld) (attacker: CombatantState) (request: CombatRequest) =
        let evidence, _ = SpatialQuery.evaluate world.Spatial (spatialRequest attacker request)
        if evidence.Explanation.CrossedCells.Length > int request.Limits.MaximumTraceCells then Error(LimitExceeded("traceCells", int32 evidence.Explanation.CrossedCells.Length, request.Limits.MaximumTraceCells))
        elif evidence.Outcome = SpatialOutcome.InvalidInput || evidence.Outcome = SpatialOutcome.Exhausted then Error(SpatialUnavailable evidence.Outcome)
        else Ok evidence
    let private candidates (world: CombatWorld) (attacker: CombatantState) (request: CombatRequest) (parameters: WeaponParameters) (evidence: SpatialQueryResult option) =
        let traceCells = evidence |> Option.map (fun item -> item.Explanation.CrossedCells) |> Option.defaultValue []
        let cells = if parameters.AreaRadius > 0 then areaCells world.Spatial request.AimCell parameters.AreaRadius else traceCells
        if cells.Length > int request.Limits.MaximumAreaCells && parameters.AreaRadius > 0 then Error(LimitExceeded("areaCells", int32 cells.Length, request.Limits.MaximumAreaCells)) else
        let index = cells |> List.mapi (fun i cell -> cell, i) |> Map.ofList
        let covers = world.Covers |> Map.toList |> List.choose (fun (id, cover) -> Map.tryFind cover.Cell index |> Option.map (fun order -> order, 0, id, CoverCandidate cover))
        let units = world.Combatants |> Map.toList |> List.choose (fun (id, unit) -> if id = attacker.EntityId then None else Map.tryFind unit.Cell index |> Option.map (fun order -> order, 1, id, CombatantCandidate unit))
        let selected = covers @ units |> List.sortBy (fun (order, kind, id, _) -> order, kind, id)
        if selected.Length > int request.Limits.MaximumRecipients then Error(LimitExceeded("recipients", int32 selected.Length, request.Limits.MaximumRecipients)) else Ok selected

    let private textBytes (value: string) = System.Text.Encoding.UTF8.GetBytes value |> fun bytes -> CanonicalEncoding.concatenate [ CanonicalEncoding.int32LittleEndian bytes.Length; bytes ]
    let private cellBytes (cell: Cell) = CanonicalEncoding.concatenate [ CanonicalEncoding.int32LittleEndian cell.Col; CanonicalEncoding.int32LittleEndian cell.Row ]
    let private profileCode = function WeaponProfile.Rifle -> 0uy | WeaponProfile.SupportWeapon -> 1uy | WeaponProfile.AntiArmor -> 2uy | WeaponProfile.LobbedArea -> 3uy
    let private arcCode = function ArmorArc.Front -> 0uy | ArmorArc.RearOrFlank -> 1uy
    let private severityCode = function WoundSeverity.Serious -> 0uy | WoundSeverity.Critical -> 1uy
    let private factBytes = function
        | Eligible id -> CanonicalEncoding.concatenate [ [| 0uy |]; textBytes id ]
        | Committed(profile, preparation) -> CanonicalEncoding.concatenate [ [| 1uy; profileCode profile |]; CanonicalEncoding.int32LittleEndian preparation ]
        | TraceEvaluated(cells, edges, bytes) -> CanonicalEncoding.concatenate ([ [| 2uy |]; CanonicalEncoding.int32LittleEndian cells.Length ] @ (cells |> List.map cellBytes) @ [ CanonicalEncoding.int32LittleEndian edges.Length; CanonicalEncoding.digest32 bytes ])
        | Contact(id, cell, index) -> CanonicalEncoding.concatenate [ [| 3uy |]; textBytes id; cellBytes cell; CanonicalEncoding.int32LittleEndian index ]
        | CoverResolved(id, cover, retained) -> CanonicalEncoding.concatenate [ [| 4uy |]; textBytes id; textBytes (Option.defaultValue "" cover); CanonicalEncoding.int32LittleEndian retained ]
        | ArmorResolved(id, arc, rating, penetration, retained) -> CanonicalEncoding.concatenate [ [| 5uy; arcCode arc |]; textBytes id; CanonicalEncoding.int32LittleEndian rating; CanonicalEncoding.int32LittleEndian penetration; CanonicalEncoding.int32LittleEndian retained ]
        | HealthChanged(id, damage, health) -> CanonicalEncoding.concatenate [ [| 6uy |]; textBytes id; CanonicalEncoding.int32LittleEndian damage; CanonicalEncoding.int32LittleEndian health ]
        | WoundApplied(id, severity) -> CanonicalEncoding.concatenate [ [| 7uy; severityCode severity |]; textBytes id ]
        | Incapacitated id -> CanonicalEncoding.concatenate [ [| 8uy |]; textBytes id ]
        | SuppressionChanged(id, delta, total) -> CanonicalEncoding.concatenate [ [| 9uy |]; textBytes id; CanonicalEncoding.int32LittleEndian delta; CanonicalEncoding.int32LittleEndian total ]
        | CoverDamaged(id, damage, integrity) -> CanonicalEncoding.concatenate [ [| 10uy |]; textBytes id; CanonicalEncoding.int32LittleEndian damage; CanonicalEncoding.int32LittleEndian integrity ]
        | CoverDestroyed id -> CanonicalEncoding.concatenate [ [| 11uy |]; textBytes id ]
    let canonicalFactsBytes facts = CanonicalEncoding.concatenate ([ CanonicalEncoding.int32LittleEndian (List.length facts) ] @ (facts |> List.map factBytes))

    let rec resolve (world: CombatWorld) (request: CombatRequest) =
        if System.String.IsNullOrWhiteSpace request.AttackId || System.String.IsNullOrWhiteSpace request.AttackerId then Error(InvalidRequest "Attack and attacker identifiers are required.")
        elif not (validLimits request.Limits) then Error(InvalidRequest "Combat limits exceed schema-v1 ceilings.")
        elif not (inBounds world.Spatial request.AimCell) then Error(InvalidRequest "Aim cell is outside the projected world.")
        else
            match Map.tryFind request.AttackerId world.Combatants with
            | None -> Error(Ineligible "Attacker does not exist in the projected combat world.")
            | Some attacker when attacker.Incapacitated -> Error(Ineligible "An incapacitated attacker cannot commit an attack.")
            | Some attacker ->
                let p = parameters request.Weapon
                let range = distance attacker.Cell request.AimCell
                if range > p.RangeCells then Error(OutOfRange(range, p.RangeCells)) else
                let evidenceResult = if p.Lobbed then Ok None else directEvidence world attacker request |> Result.map Some
                match evidenceResult with
                | Error rejection -> Error rejection
                | Ok evidence ->
                    match candidates world attacker request p evidence with
                    | Error rejection -> Error rejection
                    | Ok ordered ->
                        let preparation = 100 + range * 10
                        let startFacts = [ Committed(p.Profile, preparation); Eligible attacker.EntityId ]
                        let startFacts = match evidence with None -> startFacts | Some spatial -> TraceEvaluated(spatial.Explanation.CrossedCells, spatial.Explanation.CrossedEdges, SpatialQuery.canonicalResultBytes spatial) :: startFacts
                        let folder state (order, _, id, subject) =
                            match state with
                            | Error rejection -> Error rejection
                            | Ok(currentWorld, facts, apps, stopped) when stopped -> Ok(currentWorld, facts, apps, stopped)
                            | Ok(currentWorld, facts, apps, _) ->
                                match subject with
                                | CoverCandidate cover ->
                                    let applied = max 1 (p.BaseDamage / 2)
                                    let remaining = clamp 100 (cover.Integrity - applied)
                                    let updated = { cover with Integrity = remaining; ProjectileBlocking = cover.ProjectileBlocking && remaining > 0 }
                                    let covers = if remaining = 0 then Map.remove id currentWorld.Covers else Map.add id updated currentWorld.Covers
                                    let added = if remaining = 0 then [ CoverDestroyed id; CoverDamaged(id, applied, remaining); Contact(id, cover.Cell, int32 order) ] else [ CoverDamaged(id, applied, remaining); Contact(id, cover.Cell, int32 order) ]
                                    Ok({ currentWorld with Covers = covers }, List.rev added @ facts, apps, cover.ProjectileBlocking && p.AreaRadius = 0)
                                | CombatantCandidate target ->
                                    let precedingCover =
                                        currentWorld.Covers
                                        |> Map.toList
                                        |> List.choose (fun (coverId, cover) ->
                                            let traceIndex = evidence |> Option.bind (fun spatial -> spatial.Explanation.CrossedCells |> List.tryFindIndex ((=) cover.Cell))
                                            match traceIndex with Some ci when ci < order -> Some(ci, coverId, cover) | _ -> None)
                                        |> List.sortBy (fun (ci, coverId, _) -> ci, coverId)
                                        |> List.tryLast
                                    let coverId, coverRetained = match precedingCover with Some(_, coverId, _) -> Some coverId, 50 | None -> None, 100
                                    let arc, effectiveArmor, armorRetained = armorResolution attacker.Cell p.Penetration target.Armor target
                                    let retained = coverRetained * armorRetained / 100
                                    let damage = p.BaseDamage * retained / 100
                                    let health = clamp 100 (target.Health - damage)
                                    let suppression = clamp 100 (target.Suppression + p.Suppression)
                                    let wound =
                                        if damage >= 50 then Some { AttackId = request.AttackId; Severity = WoundSeverity.Critical; Damage = damage }
                                        elif damage >= 25 then Some { AttackId = request.AttackId; Severity = WoundSeverity.Serious; Damage = damage }
                                        else None
                                    let nextTarget = { target with Health = health; Suppression = suppression; Wounds = wound |> Option.map (fun item -> target.Wounds @ [ item ]) |> Option.defaultValue target.Wounds; Incapacitated = target.Incapacitated || health = 0 }
                                    let combat =
                                        CombatRules.resolveAttack
                                            { Attacker = attacker.Cell
                                              TargetFootprint = [ target.Cell ]
                                              VisibleSamples = (if evidence |> Option.exists (fun item -> item.Visible) || p.Lobbed then 1 else 0)
                                              TotalSamples = 1
                                              RangeCells = range
                                              Suppression = fixedRatio target.Suppression 100
                                              BaseDamage = fixedRatio p.BaseDamage 1
                                              ArmorRetention = fixedRatio retained 100
                                              EventId = request.AttackId + ":" + id }
                                        |> Result.defaultWith failwith
                                    let added =
                                        [ yield Contact(id, target.Cell, int32 order)
                                          yield CoverResolved(id, coverId, coverRetained)
                                          yield ArmorResolved(id, arc, effectiveArmor, p.Penetration, armorRetained)
                                          yield HealthChanged(id, damage, health)
                                          match wound with Some item -> yield WoundApplied(id, item.Severity) | None -> ()
                                          if nextTarget.Incapacitated && not target.Incapacitated then yield Incapacitated id
                                          yield SuppressionChanged(id, p.Suppression, suppression) ]
                                    let nextWorld = { currentWorld with Combatants = Map.add id nextTarget currentWorld.Combatants }
                                    Ok(nextWorld, List.rev added @ facts, combat.Explanation :: apps, p.AreaRadius = 0)
                        match List.fold folder (Ok(world, startFacts, [], false)) ordered with
                        | Error rejection -> Error rejection
                        | Ok(updated, facts, apps, _) ->
                            let orderedFacts = List.rev facts
                            if orderedFacts.Length > int request.Limits.MaximumFacts then Error(LimitExceeded("facts", int32 orderedFacts.Length, request.Limits.MaximumFacts)) else
                            let result = { SchemaVersion = schemaVersion; Request = request; Parameters = p; World = updated; Facts = orderedFacts; SpatialEvidence = evidence; RuleApplications = List.rev apps }
                            let byteCount = canonicalResultBytes result |> Array.length
                            if byteCount > int request.Limits.MaximumExplanationBytes then Error(LimitExceeded("explanationBytes", int32 byteCount, request.Limits.MaximumExplanationBytes)) else Ok result

    and recover (world: CombatWorld) =
        let combatants, facts =
            ((Map.empty, []), world.Combatants |> Map.toList)
            ||> List.fold (fun (next, facts) (id, unit) ->
                let recovered = min 5 unit.Suppression
                let updated = { unit with Suppression = unit.Suppression - recovered }
                let nextFacts = if recovered = 0 then facts else SuppressionChanged(id, -recovered, updated.Suppression) :: facts
                Map.add id updated next, nextFacts)
        { world with Combatants = combatants }, List.rev facts

    and canonicalResultBytes (result: CombatResult) =
        CanonicalEncoding.concatenate
            ([ CanonicalEncoding.int32LittleEndian result.SchemaVersion; textBytes result.Request.AttackId; textBytes result.Request.AttackerId; cellBytes result.Request.AimCell; [| profileCode result.Request.Weapon |]; canonicalFactsBytes result.Facts
             
             ]
             @ [ CanonicalEncoding.int32LittleEndian result.RuleApplications.Length ]
             @ (result.RuleApplications |> List.map Rules.canonicalApplicationBytes))
