module SIR.Client.Web.RulesExplorer

open Feliz
open SIR.Domain

let private valueText value =
    match value.Value with
    | IntegerValue number -> string number + " " + value.Unit
    | FixedPointValue number -> string (FixedPoint.raw number) + "/" + string FixedPoint.Scale + " " + value.Unit
    | BooleanValue flag -> string flag + " " + value.Unit
    | TextValue content -> content + " " + value.Unit

[<ReactComponent>]
let ExecutableRulesPanel () =
    let resolvedAttack, setResolvedAttack = React.useState<SIR.Simulation.SimulationEvent option>(None)
    Html.div [
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
