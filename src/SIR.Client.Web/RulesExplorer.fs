module SIR.Client.Web.RulesExplorer

open Feliz
open SIR.Client
open SIR.Domain
open SIR.Simulation

let private valueText value =
    match value.Value with
    | IntegerValue number -> string number + " " + value.Unit
    | FixedPointValue number -> string (FixedPoint.raw number) + "/" + string FixedPoint.Scale + " " + value.Unit
    | BooleanValue flag -> string flag + " " + value.Unit
    | TextValue content -> content + " " + value.Unit

let private catalogTable (label: string) (headers: string list) (rows: string list list) =
    Html.div [
        prop.className "catalog-table-scroll"
        prop.children [
            Html.table [
                prop.ariaLabel label
                prop.children [
                    Html.caption label
                    Html.thead [ Html.tr [ for header in headers do Html.th header ] ]
                    Html.tbody [ for row in rows do Html.tr [ for value in row do Html.td value ] ]
                ]
            ]
        ]
    ]

let private rulesCatalog () =
    Html.section [
        prop.children [
            Html.h2 "Units, perks, weapons, and equipment"
            Html.p "Tables label canonical, proposed, and prototype laboratory values."
            Html.details [
                prop.isOpen true
                prop.children [
                    Html.summary "Units and body profiles"
                    catalogTable "Unit roles" [ "Unit"; "Faction"; "Status"; "Role" ] [ for unit in RulesCatalog.unitRoles do [ unit.Name; unit.Faction; unit.Status; unit.Role ] ]
                    catalogTable "Prototype body profiles" [ "Body"; "Status"; "HP"; "Front armor"; "Flank armor"; "Rear armor"; "Suppression resistance"; "Regeneration/s" ] [ for body in RulesCatalog.bodyProfiles do [ body.Name; body.Status; body.Health; body.FrontArmor; body.FlankArmor; body.RearArmor; body.SuppressionResistance; body.RegenerationPerSecond ] ]
                ]
            ]
            Html.details [
                Html.summary ("Perks · " + string RulesCatalog.perkProfiles.Length)
                catalogTable "Perk families" [ "Family"; "Perk"; "Tactical change" ] [ for perk in RulesCatalog.perkProfiles do [ perk.Family; perk.Name; perk.TacticalChange ] ]
            ]
            Html.details [
                Html.summary "Weapons and prototype profiles"
                catalogTable "Canonical weapon roles" [ "Weapon"; "Engagement shape"; "Target"; "Tactical role" ] [ for weapon in RulesCatalog.weaponRoles do [ weapon.Name; weapon.EngagementShape; weapon.Target; weapon.TacticalRole ] ]
                catalogTable "Prototype weapon profiles" [ "Weapon"; "Kind"; "Base engage (s)"; "Range slope"; "Exponent"; "Accuracy"; "Dispersion/m"; "Damage"; "Penetration"; "Shots/s"; "Effect density"; "Suppression/s" ] [ for weapon in RulesCatalog.weaponProfiles do [ weapon.Name; weapon.Kind; weapon.BaseEngageSeconds; weapon.RangeSlope; weapon.Exponent; weapon.Accuracy; weapon.DispersionPerMeter; weapon.Damage; weapon.Penetration; weapon.ShotsPerSecond; weapon.EffectDensity; weapon.SuppressionPerSecond ] ]
            ]
            Html.details [
                Html.summary "Armor and equipment"
                catalogTable "Human armor packages" [ "Package"; "Coverage"; "Cost" ] [ for armor in RulesCatalog.armorProfiles do [ armor.Name; armor.Coverage; armor.Cost ] ]
                catalogTable "Equipment catalog" [ "Faction"; "Status"; "Category"; "Items" ] [ for equipment in RulesCatalog.equipmentGroups do [ equipment.Faction; equipment.Status; equipment.Category; equipment.Items ] ]
            ]
            Html.p [ Html.a [ prop.href "gameplay-reference.html"; prop.text "Read definitions, formulas, and design rationale in the Gameplay Reference." ] ]
        ]
    ]

[<ReactComponent>]
let ExecutableRulesPanel () =
    let resolvedAttack, setResolvedAttack = React.useState<SIR.Simulation.SimulationEvent option>(None)
    Html.section [
        prop.ariaLabel "Rules data tables"
        prop.children [
            Html.h2 ("Executable combat rules · " + SIR.Simulation.CombatRules.packageIdentity.PackageVersion)
            Html.button [
                prop.text "Execute canonical player attack"
                prop.onClick (fun _ ->
                    let result = SIR.Simulation.Simulation.runTick SIR.Simulation.Simulation.initialState SIR.Simulation.Simulation.inputs
                    result.Events
                    |> List.tryFind (fun (event: SIR.Simulation.SimulationEvent) ->
                        match event with
                        | SIR.Simulation.SimulationEvent.AttackResolved _ -> true
                        | _ -> false)
                    |> setResolvedAttack)
            ]
            Html.section [
                prop.ariaLabel "Authoritative attack event"
                prop.children [
                    match resolvedAttack with
                    | Some(SIR.Simulation.SimulationEvent.AttackResolved(_, _, damage, remainingHealth, explanation)) ->
                        Html.h3 ("AttackResolved event " + explanation.EventId)
                        Html.p ("Damage " + string damage + " · remaining health " + string remainingHealth + " · outcome " + valueText explanation.Outcome)
                        let governingId = RuleId.value explanation.RuleId
                        Html.a [ prop.href ("#rule-" + governingId); prop.text ("Open governing rule " + governingId) ]
                        Html.h4 "Decisive applications"
                        Html.ul [
                            for application in explanation.Children do
                                Html.li (RuleId.value application.RuleId + " · " + (application.Operands |> List.map (fun (name, value) -> name + "=" + valueText value) |> String.concat "; ") + " → " + valueText application.Outcome)
                        ]
                    | _ -> Html.p "No authoritative attack has been emitted yet."
                ]
            ]
            for rule in SIR.Simulation.CombatRules.registry do
                let metadata = rule.Metadata
                Html.details [
                    prop.id ("rule-" + RuleId.value metadata.Id)
                    if RuleId.value metadata.Id = "COMBAT-ATTACK-RESOLUTION-001" then prop.isOpen true
                    prop.children [
                        Html.summary (RuleId.value metadata.Id + " · " + metadata.Title)
                        match rule.Semantics with
                        | FormulaSemantics(_, _, expression) -> Html.p (Rules.formulaNotation expression)
                        | FactSemantics value -> Html.p ("Fact value · " + valueText value)
                        | AlgorithmSemantics contract -> Html.p ("Algorithm · " + contract.ImplementationSymbol + " · " + contract.Fingerprint + " · explains " + String.concat ", " contract.ExplanationFields)
                        | TransitionSemantics contract -> Html.p ("Transition · " + contract.Phase + " · events " + String.concat ", " contract.Events)
                        | _ -> ()
                        Html.p (metadata.Rationale + " · dependencies: " + (metadata.Dependencies |> List.map RuleId.value |> String.concat ", ") + " · evidence: " + String.concat "; " metadata.Evidence)
                        Html.p ("examples: " + String.concat "; " metadata.Examples + " · properties: " + String.concat "; " metadata.Properties)
                        Html.a [ prop.href "../../tests/fixtures/rules-corpus/v2/coverage.json"; prop.text "Coverage graph" ]
                        match metadata.RuleSource with
                        | Some source -> Html.a [ prop.href ("https://github.com/EHotwagner/S.I.R./blob/" + source.Commit + "/" + source.RepositoryPath); prop.text ("Pinned F# source · " + source.Symbol) ]
                        | None -> Html.span "No executable source"
                    ]
                ]
        ]
    ]

[<ReactComponent>]
let DeferredDataPanel (simulator: SimulatorHandoff option, selectedUnit: int32 option) =
    let selected =
        simulator
        |> Option.bind (fun state ->
            selectedUnit
            |> Option.bind (fun id -> Map.tryFind id state.RuntimeMap.Units)
            |> Option.map (fun unit -> state, unit))

    Html.div [
        prop.children [
            Html.section [
                prop.ariaLabel "Selected unit spatial diagnostics"
                prop.children [
                    Html.h2 "Spatial diagnostics"
                    match selected with
                    | None -> Html.p "Select a simulator unit to inspect authoritative spatial queries."
                    | Some(state, unit) ->
                        let cell col row: FS.GG.Game.Core.Cell = { Col = col; Row = row }
                        let identity =
                            SpatialAuthorityIdentity.create state.Revision.Digest "sir-spatial-v1" state.Revision.Number "player-disclosed" state.Revision.Number
                            |> Result.defaultWith failwith
                        let world =
                            { Identity = identity
                              Minimum = cell 0 0
                              Maximum = cell (state.RuntimeMap.Width - 1) (state.RuntimeMap.Height - 1)
                              Terrain =
                                state.RuntimeMap.Terrain
                                |> Map.toList
                                |> List.map (fun ((column, row), terrain) ->
                                    cell column row,
                                    match terrain with
                                    | MapTerrain.Open | MapTerrain.Objective -> SpatialTerrain.Open
                                    | MapTerrain.Rough -> SpatialTerrain.Rough
                                    | MapTerrain.Blocked -> SpatialTerrain.Blocked)
                                |> Map.ofList
                              Boundaries = []
                              Occupancy = Map.empty
                              DisclosedRevisionTokens = Set.empty }
                        let origin = cell unit.Column unit.Row
                        let target = cell (min (state.RuntimeMap.Width - 1) (unit.Column + 4)) unit.Row
                        let request =
                            { QueryId = "selected-unit-diagnostics"
                              QueryKind = SpatialQueryKind.ExactLineOfSight
                              Origin = origin
                              Target = target
                              Footprint = [ for row in 0 .. unit.Size - 1 do for column in 0 .. unit.Size - 1 do yield cell column row ]
                              Profile = { ProfileId = "selected-unit-sensor-v1"; Modality = SpatialModality.Vision; Stance = "standing"; HeightBand = 1; Facing = unit.AttentionDirection }
                              Bounds = SpatialQuery.defaultBounds }
                        let result, _ = SpatialQuery.evaluate world request
                        Html.dl [
                            Html.dt "Query"
                            Html.dd (string result.Explanation.QueryKind)
                            Html.dt "Outcome"
                            Html.dd (string result.Outcome)
                            Html.dt "Footprint samples"
                            Html.dd (string result.Explanation.FootprintSamples.Length)
                            Html.dt "Crossed cells / edges"
                            Html.dd (string result.Explanation.CrossedCells.Length + " / " + string result.Explanation.CrossedEdges.Length)
                            Html.dt "Cover contributors"
                            Html.dd (string result.Explanation.CoverContributors.Length)
                            Html.dt "Exposure directions"
                            Html.dd (result.Explanation.ExposureDirections |> List.map string |> String.concat ", ")
                            Html.dt "Revision / knowledge policy"
                            Html.dd (string result.Explanation.SpatialRevision + " / " + result.Explanation.KnowledgeIdentity)
                            Html.dt "Package / profile"
                            Html.dd (SpatialQuery.packageIdentity + " / " + SpatialQuery.compatibilityProfile)
                            Html.dt "Pinned authority"
                            Html.dd result.Explanation.SourceSymbol
                        ]
                ]
            ]
            ExecutableRulesPanel ()
            rulesCatalog ()
        ]
    ]
