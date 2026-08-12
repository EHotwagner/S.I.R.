module SIR.Client.Web.RulesExplorer

open Feliz
open Fable.Core
open Fable.Core.JsInterop
open SIR.Client
open SIR.Domain
open SIR.Protocol.Http
open Thoth.Json

type private SpatialDiagnosticCell =
    { Column: int32
      Row: int32 }

type private SpatialDiagnosticEdge =
    { Low: SpatialDiagnosticCell
      High: SpatialDiagnosticCell }

type private SpatialDiagnosticResult =
    { QueryId: string
      QueryKind: string
      Outcome: string
      Origin: SpatialDiagnosticCell
      Target: SpatialDiagnosticCell
      FootprintSamples: SpatialDiagnosticCell list
      Path: SpatialDiagnosticCell list
      MovementCost: int32
      Visible: bool
      CrossedCells: SpatialDiagnosticCell list
      CrossedEdges: SpatialDiagnosticEdge list
      CoverContributors: SpatialDiagnosticEdge list
      ExposureDirections: string list
      Decisions: string list
      Expansions: int32
      Truncated: bool
      SpatialRevision: int64
      KnowledgeIdentity: string
      KnowledgeRevision: int64
      ProfileId: string
      PackageIdentity: string
      CompatibilityProfile: string
      SourceSymbol: string }

type private SpatialDiagnosticProjection =
    { Queries: SpatialDiagnosticResult list }

[<Global>]
let private fetch (url: string, options: obj) : JS.Promise<obj> = jsNative

let private cellDecoder : Decoder<SpatialDiagnosticCell> =
    Decode.object (fun get ->
        { Column = get.Required.Field "Column" Decode.int
          Row = get.Required.Field "Row" Decode.int })

let private edgeDecoder : Decoder<SpatialDiagnosticEdge> =
    Decode.object (fun get ->
        { Low = get.Required.Field "Low" cellDecoder
          High = get.Required.Field "High" cellDecoder })

let private resultDecoder : Decoder<SpatialDiagnosticResult> =
    Decode.object (fun get ->
        { QueryId = get.Required.Field "QueryId" Decode.string
          QueryKind = get.Required.Field "QueryKind" Decode.string
          Outcome = get.Required.Field "Outcome" Decode.string
          Origin = get.Required.Field "Origin" cellDecoder
          Target = get.Required.Field "Target" cellDecoder
          FootprintSamples = get.Required.Field "FootprintSamples" (Decode.list cellDecoder)
          Path = get.Required.Field "Path" (Decode.list cellDecoder)
          MovementCost = get.Required.Field "MovementCost" Decode.int
          Visible = get.Required.Field "Visible" Decode.bool
          CrossedCells = get.Required.Field "CrossedCells" (Decode.list cellDecoder)
          CrossedEdges = get.Required.Field "CrossedEdges" (Decode.list edgeDecoder)
          CoverContributors = get.Required.Field "CoverContributors" (Decode.list edgeDecoder)
          ExposureDirections = get.Required.Field "ExposureDirections" (Decode.list Decode.string)
          Decisions = get.Required.Field "Decisions" (Decode.list Decode.string)
          Expansions = get.Required.Field "Expansions" Decode.int
          Truncated = get.Required.Field "Truncated" Decode.bool
          SpatialRevision = get.Required.Field "SpatialRevision" Decode.int64
          KnowledgeIdentity = get.Required.Field "KnowledgeIdentity" Decode.string
          KnowledgeRevision = get.Required.Field "KnowledgeRevision" Decode.int64
          ProfileId = get.Required.Field "ProfileId" Decode.string
          PackageIdentity = get.Required.Field "PackageIdentity" Decode.string
          CompatibilityProfile = get.Required.Field "CompatibilityProfile" Decode.string
          SourceSymbol = get.Required.Field "SourceSymbol" Decode.string })

let private responseDecoder : Decoder<SpatialDiagnosticProjection> =
    Decode.object (fun get ->
        { Queries = get.Required.Field "Queries" (Decode.list resultDecoder) })

let private cellText value = $"({value.Column},{value.Row})"
let private edgeText value = cellText value.Low + "→" + cellText value.High
let private valuesText render values =
    match values with
    | [] -> "none"
    | _ -> values |> List.map render |> String.concat ", "

let private terrainCode terrain =
    match terrain with
    | MapTerrain.Open
    | MapTerrain.Objective -> 0
    | MapTerrain.Rough -> 1
    | MapTerrain.Blocked -> 2

let private requestBody (simulator: SimulatorHandoff option) selectedUnit =
    match simulator, selectedUnit with
    | Some handoff, Some unitId ->
        match Map.tryFind unitId handoff.RuntimeMap.Units with
        | None -> None
        | Some unit ->
            Some(
                Encode.object [
                    "MapIdentity", Encode.string handoff.Revision.Digest
                    "SpatialRevision", Encode.int64 handoff.Revision.Number
                    "Width", Encode.int handoff.RuntimeMap.Width
                    "Height", Encode.int handoff.RuntimeMap.Height
                    "OriginColumn", Encode.int unit.Column
                    "OriginRow", Encode.int unit.Row
                    "UnitSize", Encode.int unit.Size
                    "Facing", Encode.int (int32 (Direction8.toCode unit.AttentionDirection))
                    "Terrain",
                        handoff.RuntimeMap.Terrain
                        |> Map.toList
                        |> List.map (fun ((column, row), terrain) ->
                            Encode.object [
                                "Column", Encode.int column
                                "Row", Encode.int row
                                "Kind", Encode.int (terrainCode terrain)
                            ])
                        |> Encode.list
                ]
                |> Encode.toString 0)
    | _ -> None

let private loadDiagnostics accessToken body =
    async {
        let options =
            createObj [
                "method" ==> "POST"
                "headers" ==> createObj [ "Content-Type" ==> "application/json"; "Authorization" ==> ("Bearer " + accessToken) ]
                "body" ==> body
            ]
        let! response = fetch ("/api/spatial/diagnostics", options) |> Async.AwaitPromise
        let! responseBody = response?text() |> Async.AwaitPromise
        if not (unbox<bool> response?ok) then
            failwith $"spatial diagnostic request failed: {responseBody}"
        return
            Decode.fromString responseDecoder (string responseBody)
            |> Result.defaultWith (fun error -> failwith $"spatial diagnostic response did not decode: {error}")
    }

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
let DeferredDataPanel (simulator: SimulatorHandoff option) (selectedUnit: int32 option) (bootstrap: BootstrapV1.Response option) =
    let request = requestBody simulator selectedUnit
    let diagnostics, setDiagnostics = React.useState<SpatialDiagnosticProjection option>(None)
    let failure, setFailure = React.useState<string option>(None)
    React.useEffect(
        (fun () ->
            setDiagnostics None
            setFailure None
            match request, bootstrap with
            | Some body, Some admission ->
                Async.StartImmediate(async {
                    try
                        let! result = loadDiagnostics admission.AccessToken body
                        setDiagnostics (Some result)
                    with error ->
                        setFailure (Some error.Message)
                })
            | _ -> ()),
        [| box request; box bootstrap |])
    Html.div [
        prop.children [
            Html.section [
                prop.ariaLabel "Selected unit spatial diagnostics"
                prop.children [
                    Html.h2 "Spatial diagnostics"
                    match request, failure, diagnostics with
                    | None, _, _ -> Html.p "Select a simulator unit to inspect authoritative spatial queries."
                    | Some _, Some error, _ -> Html.p ("Authoritative spatial diagnostics unavailable: " + error)
                    | Some _, None, None -> Html.p "Loading authoritative spatial diagnostics."
                    | Some _, None, Some result ->
                        Html.div [
                            prop.children [
                                for query in result.Queries do
                                    Html.details [
                                        prop.isOpen true
                                        prop.children [
                                            Html.summary (query.QueryKind + " · " + query.Outcome)
                                            Html.dl [
                                                Html.dt "Normalized inputs"
                                                Html.dd (query.QueryId + " · " + cellText query.Origin + " → " + cellText query.Target + " · " + query.ProfileId)
                                                Html.dt "Footprint samples"
                                                Html.dd (valuesText cellText query.FootprintSamples)
                                                Html.dt "Authoritative path"
                                                Html.dd (valuesText cellText query.Path)
                                                Html.dt "Crossed cells"
                                                Html.dd (valuesText cellText query.CrossedCells)
                                                Html.dt "Crossed edges"
                                                Html.dd (valuesText edgeText query.CrossedEdges)
                                                Html.dt "Cover contributors"
                                                Html.dd (valuesText edgeText query.CoverContributors)
                                                Html.dt "Exposure directions"
                                                Html.dd (valuesText id query.ExposureDirections)
                                                Html.dt "Decisions"
                                                Html.dd (String.concat ", " query.Decisions)
                                                Html.dt "Expansion / truncation"
                                                Html.dd (string query.Expansions + " / " + string query.Truncated)
                                                Html.dt "Revision / knowledge policy"
                                                Html.dd (string query.SpatialRevision + " / " + query.KnowledgeIdentity + "@" + string query.KnowledgeRevision)
                                                Html.dt "Package / profile"
                                                Html.dd (query.PackageIdentity + " / " + query.CompatibilityProfile)
                                                Html.dt "Pinned authority"
                                                Html.dd query.SourceSymbol
                                            ]
                                        ]
                                    ]
                            ]
                        ]
                ]
            ]
            ExecutableRulesPanel ()
            rulesCatalog ()
        ]
    ]
