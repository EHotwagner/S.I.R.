
import { int32ToString } from "./fable_modules/fable-library-js.5.13.0/Util.js";
import { FixedPointModule_raw } from "./SIR.Domain/FixedPoint.js";
import { toString } from "./fable_modules/fable-library-js.5.13.0/Types.js";
import { createElement, useState } from "react";
import React from "react";
import { HtmlHelper_createElement } from "./fable_modules/Feliz.3.3.3/Html.fs.js";
import { empty, collect, map, singleton, append, delay, toList } from "./fable_modules/fable-library-js.5.13.0/Seq.js";
import { CombatRules_registry, CombatRules_packageIdentity } from "./SIR.Simulation/CombatRules.js";
import { defaultOf } from "./fable_modules/fable-library-js.5.13.0/Util.js";
import { map as map_1, ofArray, tryFind, singleton as singleton_1 } from "./fable_modules/fable-library-js.5.13.0/List.js";
import { Simulation_inputs, Simulation_initialState, Simulation_runTick } from "./SIR.Simulation/Simulation.js";
import { RuleIdModule_value } from "./SIR.Domain/RuleTypes.js";
import { join } from "./fable_modules/fable-library-js.5.13.0/String.js";
import { formulaNotation } from "./SIR.Domain/Rules.js";

function valueText(value) {
    const matchValue = value.Value;
    switch (matchValue.tag) {
        case 1:
            return (((int32ToString(FixedPointModule_raw(matchValue.fields[0])) + "/") + int32ToString(10000)) + " ") + value.Unit;
        case 2:
            return (toString(matchValue.fields[0]) + " ") + value.Unit;
        case 3:
            return (matchValue.fields[0] + " ") + value.Unit;
        default:
            return (int32ToString(matchValue.fields[0]) + " ") + value.Unit;
    }
}

export function ExecutableRulesPanel() {
    const patternInput = useState(undefined);
    const resolvedAttack = patternInput[0];
    return HtmlHelper_createElement("div", singleton_1(["children", toList(delay(() => {
        let value;
        return append(singleton((value = ("Executable combat rules · " + CombatRules_packageIdentity.PackageVersion), createElement("h2", defaultOf(), value))), delay(() => append(singleton(HtmlHelper_createElement("button", ofArray([["children", singleton_1("Execute canonical player attack")], ["onClick", (_arg) => {
            patternInput[1](tryFind((event) => {
                if (event.tag === 3) {
                    return true;
                }
                else {
                    return false;
                }
            }, Simulation_runTick(Simulation_initialState, Simulation_inputs).Events));
        }]]))), delay(() => append(singleton(HtmlHelper_createElement("section", ofArray([["aria-label", "Authoritative attack event"], ["children", toList(delay(() => {
            let value_6;
            let matchResult, damage, explanation, remainingHealth;
            if (resolvedAttack != null) {
                if (resolvedAttack.tag === 3) {
                    matchResult = 0;
                    damage = resolvedAttack.fields[2];
                    explanation = resolvedAttack.fields[4];
                    remainingHealth = resolvedAttack.fields[3];
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
            switch (matchResult) {
                case 0:
                    return append(singleton((value_6 = ("AttackResolved event " + explanation.EventId), createElement("h3", defaultOf(), value_6))), delay(() => {
                        let value_7;
                        return append(singleton((value_7 = ((((("Damage " + int32ToString(damage)) + " · remaining health ") + int32ToString(remainingHealth)) + " · outcome ") + valueText(explanation.Outcome)), createElement("p", defaultOf(), value_7))), delay(() => {
                            const governingId = RuleIdModule_value(explanation.RuleId);
                            return append(singleton(HtmlHelper_createElement("a", ofArray([["href", "#rule-" + governingId], ["children", singleton_1("Open governing rule " + governingId)]]))), delay(() => append(singleton(createElement("h4", defaultOf(), "Decisive applications")), delay(() => {
                                let children_10;
                                return singleton((children_10 = toList(delay(() => map((application) => {
                                    const value_14 = (((RuleIdModule_value(application.RuleId) + " · ") + join("; ", map_1((tupledArg) => ((tupledArg[0] + "=") + valueText(tupledArg[1])), application.Operands))) + " → ") + valueText(application.Outcome);
                                    return createElement("li", defaultOf(), value_14);
                                }, explanation.Children))), createElement("ul", defaultOf(), ...children_10)));
                            }))));
                        }));
                    }));
                default:
                    return singleton(createElement("p", defaultOf(), "No authoritative attack has been emitted yet."));
            }
        }))]]))), delay(() => collect((rule) => {
            const metadata = rule.Metadata;
            return singleton(HtmlHelper_createElement("details", toList(delay(() => append(singleton(["id", "rule-" + RuleIdModule_value(metadata.Id)]), delay(() => append((RuleIdModule_value(metadata.Id) === "COMBAT-ATTACK-RESOLUTION-001") ? singleton(["open", true]) : empty(), delay(() => singleton(["children", toList(delay(() => {
                let value_21;
                return append(singleton((value_21 = ((RuleIdModule_value(metadata.Id) + " · ") + metadata.Title), createElement("summary", defaultOf(), value_21))), delay(() => {
                    let matchValue, value_22, value_24, contract, value_25, contract_1, value_26;
                    return append((matchValue = rule.Semantics, (matchValue.tag === 2) ? singleton((value_22 = formulaNotation(matchValue.fields[2]), createElement("p", defaultOf(), value_22))) : ((matchValue.tag === 0) ? singleton((value_24 = ("Fact value · " + valueText(matchValue.fields[0])), createElement("p", defaultOf(), value_24))) : ((matchValue.tag === 4) ? ((contract = matchValue.fields[0], singleton((value_25 = ((((("Algorithm · " + contract.ImplementationSymbol) + " · ") + contract.Fingerprint) + " · explains ") + join(", ", contract.ExplanationFields)), createElement("p", defaultOf(), value_25))))) : ((matchValue.tag === 3) ? ((contract_1 = matchValue.fields[0], singleton((value_26 = ((("Transition · " + contract_1.Phase) + " · events ") + join(", ", contract_1.Events)), createElement("p", defaultOf(), value_26))))) : (empty()))))), delay(() => {
                        let value_27;
                        return append(singleton((value_27 = ((((metadata.Rationale + " · dependencies: ") + join(", ", map_1(RuleIdModule_value, metadata.Dependencies))) + " · evidence: ") + join("; ", metadata.Evidence)), createElement("p", defaultOf(), value_27))), delay(() => {
                            let value_28;
                            return append(singleton((value_28 = ((("examples: " + join("; ", metadata.Examples)) + " · properties: ") + join("; ", metadata.Properties)), createElement("p", defaultOf(), value_28))), delay(() => append(singleton(HtmlHelper_createElement("a", ofArray([["href", "../../tests/fixtures/rules-corpus/v2/coverage.json"], ["children", singleton_1("Coverage graph")]]))), delay(() => {
                                const matchValue_1 = metadata.RuleSource;
                                if (matchValue_1 == null) {
                                    return singleton(createElement("span", defaultOf(), "No executable source"));
                                }
                                else {
                                    const source = matchValue_1;
                                    return singleton(HtmlHelper_createElement("a", ofArray([["href", (("https://github.com/EHotwagner/S.I.R./blob/" + source.Commit) + "/") + source.RepositoryPath], ["children", singleton_1("Pinned F# source · " + source.Symbol)]])));
                                }
                            }))));
                        }));
                    }));
                }));
            }))])))))))));
        }, CombatRules_registry)))))));
    }))]));
}

