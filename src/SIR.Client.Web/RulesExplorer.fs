module SIR.Client.Web.RulesExplorer

open Feliz
open SIR.Domain

let private valueText value =
    match value.Value with
    | IntegerValue number -> string number + " " + value.Unit
    | FixedPointValue number -> string (FixedPoint.raw number) + "/" + string FixedPoint.Scale + " " + value.Unit
    | BooleanValue flag -> string flag + " " + value.Unit
    | TextValue content -> content + " " + value.Unit

let private representativeAttack =
    SIR.Simulation.CombatRules.resolveAttack
        { Attacker = { Col = 0; Row = 0 }
          TargetFootprint = [ { Col = 1; Row = 0 }; { Col = 1; Row = 1 } ]
          IsTransparent = fun _ -> true
          RangeCells = 1
          Suppression = FixedPoint.zero
          BaseDamage = FixedPoint.fromRatio 25 1 |> Result.defaultWith (fun _ -> failwith "invalid representative damage")
          ArmorRetention = FixedPoint.fromRatio 4 5 |> Result.defaultWith (fun _ -> failwith "invalid representative retention")
          EventId = "fixture-attack-1" }
    |> Result.defaultWith failwith

[<ReactComponent>]
let ExecutableRulesPanel () =
    Html.div [
        prop.children [
            Html.h2 ("Executable combat rules · " + SIR.Simulation.CombatRules.packageIdentity.PackageVersion)
            Html.section [
                prop.ariaLabel "Authoritative attack event"
                prop.children [
                    Html.h3 "AttackResolved event fixture-attack-1"
                    Html.p ("Outcome · " + valueText representativeAttack.Explanation.Outcome)
                    Html.a [ prop.href "#rule-COMBAT-ATTACK-RESOLUTION-001"; prop.text "Open governing rule COMBAT-ATTACK-RESOLUTION-001" ]
                    Html.h4 "Decisive applications"
                    Html.ul [
                        for application in representativeAttack.Explanation.Children do
                            Html.li (RuleId.value application.RuleId + " · " + (application.Operands |> List.map (fun (name, value) -> name + "=" + valueText value) |> String.concat "; ") + " → " + valueText application.Outcome)
                    ]
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
