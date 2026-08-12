
import { toString, Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { array_type, option_type, class_type, list_type, record_type, int32_type, string_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { empty, mapIndexed, length, append, toArray as toArray_1, pick, choose, tryPick, map as map_1, tryFind, ofArray } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { ofArray as ofArray_1, toArray, find, map as map_3, add, fold, toList, tryFind as tryFind_1, ofList } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { Exception, int32ToString, comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { Result_DefaultWith, FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { format, join } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { initialize, take, map as map_2 } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { sha256 } from "../SIR.Domain/CanonicalHash.js";
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";
import { BoundedInt32Module_value, BoundedInt32Module_create } from "../SIR.Domain/BoundedInt32.js";
import { Simulation_runTickWithRules, Simulation_initialState, Simulation_inputs, SimulationRules } from "../SIR.Simulation/Simulation.js";
import { toList as toList_1 } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";
import { max } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { RenderFrame, DisclosureLabel, RenderEventVisual, EdgeVisual, UnitVisual, Disclosure$1, FactionVisual, UnitClassIdModule_placeholder, BoardVisual, HealthVisualModule_tryCreate, CellExtentModule_tryCreate } from "./ReplayPresentation.js";
import { defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";

export class LabParameter extends Record {
    constructor(Key, Label, Minimum, Maximum, Step, DefaultValue) {
        super();
        this.Key = Key;
        this.Label = Label;
        this.Minimum = (Minimum | 0);
        this.Maximum = (Maximum | 0);
        this.Step = (Step | 0);
        this.DefaultValue = (DefaultValue | 0);
    }
}

export function LabParameter_$reflection() {
    return record_type("SIR.Client.LabParameter", [], LabParameter, () => [["Key", string_type], ["Label", string_type], ["Minimum", int32_type], ["Maximum", int32_type], ["Step", int32_type], ["DefaultValue", int32_type]]);
}

export class DesignScenario extends Record {
    constructor(Identity, Revision, Title, Description, EngineIdentity, RulesetIdentity, Parameters) {
        super();
        this.Identity = Identity;
        this.Revision = (Revision | 0);
        this.Title = Title;
        this.Description = Description;
        this.EngineIdentity = EngineIdentity;
        this.RulesetIdentity = RulesetIdentity;
        this.Parameters = Parameters;
    }
}

export function DesignScenario_$reflection() {
    return record_type("SIR.Client.DesignScenario", [], DesignScenario, () => [["Identity", string_type], ["Revision", int32_type], ["Title", string_type], ["Description", string_type], ["EngineIdentity", string_type], ["RulesetIdentity", string_type], ["Parameters", list_type(LabParameter_$reflection())]]);
}

export class ExperimentInput extends Record {
    constructor(ScenarioIdentity, ScenarioRevision, EngineIdentity, RulesetIdentity, Parameters) {
        super();
        this.ScenarioIdentity = ScenarioIdentity;
        this.ScenarioRevision = (ScenarioRevision | 0);
        this.EngineIdentity = EngineIdentity;
        this.RulesetIdentity = RulesetIdentity;
        this.Parameters = Parameters;
    }
}

export function ExperimentInput_$reflection() {
    return record_type("SIR.Client.ExperimentInput", [], ExperimentInput, () => [["ScenarioIdentity", string_type], ["ScenarioRevision", int32_type], ["EngineIdentity", string_type], ["RulesetIdentity", string_type], ["Parameters", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, int32_type])]]);
}

export class ExperimentResult extends Record {
    constructor(Input, ResultIdentity, Metrics) {
        super();
        this.Input = Input;
        this.ResultIdentity = ResultIdentity;
        this.Metrics = Metrics;
    }
}

export function ExperimentResult_$reflection() {
    return record_type("SIR.Client.ExperimentResult", [], ExperimentResult, () => [["Input", ExperimentInput_$reflection()], ["ResultIdentity", string_type], ["Metrics", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, int32_type])]]);
}

export class ExperimentComparison extends Record {
    constructor(Baseline, Fork, Delta) {
        super();
        this.Baseline = Baseline;
        this.Fork = Fork;
        this.Delta = Delta;
    }
}

export function ExperimentComparison_$reflection() {
    return record_type("SIR.Client.ExperimentComparison", [], ExperimentComparison, () => [["Baseline", ExperimentResult_$reflection()], ["Fork", ExperimentResult_$reflection()], ["Delta", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, int32_type])]]);
}

export class SweepResult extends Record {
    constructor(Parameter, Results) {
        super();
        this.Parameter = Parameter;
        this.Results = Results;
    }
}

export function SweepResult_$reflection() {
    return record_type("SIR.Client.SweepResult", [], SweepResult, () => [["Parameter", string_type], ["Results", list_type(ExperimentResult_$reflection())]]);
}

export class LabReport extends Record {
    constructor(Comparison, Sweep, EvidenceLabel) {
        super();
        this.Comparison = Comparison;
        this.Sweep = Sweep;
        this.EvidenceLabel = EvidenceLabel;
    }
}

export function LabReport_$reflection() {
    return record_type("SIR.Client.LabReport", [], LabReport, () => [["Comparison", ExperimentComparison_$reflection()], ["Sweep", option_type(SweepResult_$reflection())], ["EvidenceLabel", string_type]]);
}

/**
 * A structured-clone-safe key/value entry for the browser worker boundary.
 */
export class Int32Entry extends Record {
    constructor(Key, Value) {
        super();
        this.Key = Key;
        this.Value = (Value | 0);
    }
}

export function Int32Entry_$reflection() {
    return record_type("SIR.Client.Int32Entry", [], Int32Entry, () => [["Key", string_type], ["Value", int32_type]]);
}

export class DesignScenarioTransport extends Record {
    constructor(Identity, Revision, Title, Description, EngineIdentity, RulesetIdentity, Parameters) {
        super();
        this.Identity = Identity;
        this.Revision = (Revision | 0);
        this.Title = Title;
        this.Description = Description;
        this.EngineIdentity = EngineIdentity;
        this.RulesetIdentity = RulesetIdentity;
        this.Parameters = Parameters;
    }
}

export function DesignScenarioTransport_$reflection() {
    return record_type("SIR.Client.DesignScenarioTransport", [], DesignScenarioTransport, () => [["Identity", string_type], ["Revision", int32_type], ["Title", string_type], ["Description", string_type], ["EngineIdentity", string_type], ["RulesetIdentity", string_type], ["Parameters", array_type(LabParameter_$reflection())]]);
}

export class ExperimentInputTransport extends Record {
    constructor(ScenarioIdentity, ScenarioRevision, EngineIdentity, RulesetIdentity, Parameters) {
        super();
        this.ScenarioIdentity = ScenarioIdentity;
        this.ScenarioRevision = (ScenarioRevision | 0);
        this.EngineIdentity = EngineIdentity;
        this.RulesetIdentity = RulesetIdentity;
        this.Parameters = Parameters;
    }
}

export function ExperimentInputTransport_$reflection() {
    return record_type("SIR.Client.ExperimentInputTransport", [], ExperimentInputTransport, () => [["ScenarioIdentity", string_type], ["ScenarioRevision", int32_type], ["EngineIdentity", string_type], ["RulesetIdentity", string_type], ["Parameters", array_type(Int32Entry_$reflection())]]);
}

export class ExperimentResultTransport extends Record {
    constructor(Input, ResultIdentity, Metrics) {
        super();
        this.Input = Input;
        this.ResultIdentity = ResultIdentity;
        this.Metrics = Metrics;
    }
}

export function ExperimentResultTransport_$reflection() {
    return record_type("SIR.Client.ExperimentResultTransport", [], ExperimentResultTransport, () => [["Input", ExperimentInputTransport_$reflection()], ["ResultIdentity", string_type], ["Metrics", array_type(Int32Entry_$reflection())]]);
}

export class SweepResultTransport extends Record {
    constructor(Parameter, Results) {
        super();
        this.Parameter = Parameter;
        this.Results = Results;
    }
}

export function SweepResultTransport_$reflection() {
    return record_type("SIR.Client.SweepResultTransport", [], SweepResultTransport, () => [["Parameter", string_type], ["Results", array_type(ExperimentResultTransport_$reflection())]]);
}

export class LabReportTransport extends Record {
    constructor(Baseline, Fork, Delta, Sweep, EvidenceLabel) {
        super();
        this.Baseline = Baseline;
        this.Fork = Fork;
        this.Delta = Delta;
        this.Sweep = Sweep;
        this.EvidenceLabel = EvidenceLabel;
    }
}

export function LabReportTransport_$reflection() {
    return record_type("SIR.Client.LabReportTransport", [], LabReportTransport, () => [["Baseline", ExperimentResultTransport_$reflection()], ["Fork", ExperimentResultTransport_$reflection()], ["Delta", array_type(Int32Entry_$reflection())], ["Sweep", option_type(SweepResultTransport_$reflection())], ["EvidenceLabel", string_type]]);
}

const Lab_engineIdentity = "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";

const Lab_rulesetIdentity = "6d31302d72756c65732d6c61622d763100000000000000000000000000000000";

function Lab_attackParameters(attackPower, attackCount) {
    return ofArray([new LabParameter("attack-power", "Attack power", 1, 100, 1, attackPower), new LabParameter("attack-count", "Attack count", 1, 8, 1, attackCount)]);
}

export const Lab_catalog = ofArray([new DesignScenario("adjacent-duel", 1, "Four-hit baseline", "Four standard attacks establish the immutable comparison baseline.", Lab_engineIdentity, Lab_rulesetIdentity, Lab_attackParameters(25, 4)), new DesignScenario("short-duel", 1, "Two-hit exchange", "Two standard attacks show the target surviving at half health.", Lab_engineIdentity, Lab_rulesetIdentity, Lab_attackParameters(25, 2)), new DesignScenario("single-heavy-strike", 1, "Single heavy strike", "One high-power attack makes damage scaling easy to inspect.", Lab_engineIdentity, Lab_rulesetIdentity, Lab_attackParameters(60, 1)), new DesignScenario("rapid-chip-damage", 1, "Rapid chip damage", "Eight low-power attacks expose accumulation without reaching lethal damage.", Lab_engineIdentity, Lab_rulesetIdentity, Lab_attackParameters(8, 8)), new DesignScenario("lethality-threshold", 1, "Lethality threshold", "Three 34-power attacks cross the exact 100-health defeat threshold.", Lab_engineIdentity, Lab_rulesetIdentity, Lab_attackParameters(34, 3)), new DesignScenario("near-threshold", 1, "Near-threshold survivor", "Three 33-power attacks deliberately leave the target on one health.", Lab_engineIdentity, Lab_rulesetIdentity, Lab_attackParameters(33, 3))]);

export function Lab_tryScenario(identity) {
    return tryFind((scenario) => (scenario.Identity === identity), Lab_catalog);
}

export function Lab_defaults(scenario) {
    return ofList(map_1((parameter) => [parameter.Key, parameter.DefaultValue], scenario.Parameters), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
}

export function Lab_validate(scenario, patch) {
    const definitions = ofList(map_1((parameter) => [parameter.Key, parameter], scenario.Parameters), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const _arg = tryPick((tupledArg) => {
        let parameter_1, parameter_2;
        const key = tupledArg[0];
        const value = tupledArg[1] | 0;
        const matchValue = tryFind_1(key, definitions);
        if (matchValue != null) {
            if ((parameter_1 = matchValue, (value < parameter_1.Minimum) ? true : (value > parameter_1.Maximum))) {
                const parameter_3 = matchValue;
                return ((((parameter_3.Label + " must be between ") + int32ToString(parameter_3.Minimum)) + " and ") + int32ToString(parameter_3.Maximum)) + ".";
            }
            else if ((parameter_2 = matchValue, ((value - parameter_2.Minimum) % parameter_2.Step) !== 0)) {
                const parameter_4 = matchValue;
                return ((parameter_4.Label + " must use step ") + int32ToString(parameter_4.Step)) + ".";
            }
            else {
                return undefined;
            }
        }
        else {
            return "Unknown parameter: " + key;
        }
    }, toList(patch));
    if (_arg == null) {
        return new FSharpResult$2(/* Ok */ 0, [fold((values, key_1, value_1) => add(key_1, value_1, values), Lab_defaults(scenario), patch)]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [_arg]);
    }
}

function Lab_required(key, parameters) {
    const option_1 = tryFind_1(key, parameters);
    if (option_1 != null) {
        return option_1 | 0;
    }
    else {
        throw new Exception("Missing laboratory parameter: " + key);
    }
}

function Lab_identityOf(input, metrics) {
    let chars, objectArg;
    return join("", map_2((value_2) => format('{0:' + "x2" + '}', value_2), take(8, sha256((chars = join("|", [input.ScenarioIdentity, int32ToString(input.ScenarioRevision), input.EngineIdentity, input.RulesetIdentity, join(";", map_1((tupledArg) => ((tupledArg[0] + "=") + int32ToString(tupledArg[1])), toList(input.Parameters))), join(";", map_1((tupledArg_1) => ((tupledArg_1[0] + "=") + int32ToString(tupledArg_1[1])), toList(metrics)))]), (objectArg = get_UTF8(), objectArg.getBytes(chars)))), Uint8Array)));
}

export function Lab_evaluate(scenario, parameters) {
    const attackPower = Lab_required("attack-power", parameters) | 0;
    const attackCount = Lab_required("attack-count", parameters) | 0;
    const rules = new SimulationRules(Result_DefaultWith((error) => {
        throw new Exception("Validated attack power could not enter the kernel: " + toString(error));
    }, BoundedInt32Module_create(0, 100, attackPower)));
    const attackOnly = choose((_arg) => {
        if (_arg.tag === 2) {
            return _arg;
        }
        else {
            return undefined;
        }
    }, Simulation_inputs);
    let state = Simulation_initialState;
    for (let index = 1; index <= attackCount; index++) {
        const journal = (index === 1) ? Simulation_inputs : attackOnly;
        state = Simulation_runTickWithRules(rules, state, journal).State;
    }
    const remainingHealth = pick((tupledArg) => {
        const unit = tupledArg[1];
        if (unit.Side.tag === 0) {
            return undefined;
        }
        else {
            return BoundedInt32Module_value(unit.Health);
        }
    }, toList(state.Units)) | 0;
    const input = new ExperimentInput(scenario.Identity, scenario.Revision, scenario.EngineIdentity, scenario.RulesetIdentity, parameters);
    const metrics = ofList(ofArray([["attack-events", attackCount], ["remaining-health", remainingHealth], ["total-damage", 100 - remainingHealth]]), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    return new ExperimentResult(input, Lab_identityOf(input, metrics), metrics);
}

export function Lab_run(scenario, patch, sweepParameter) {
    let option_3, key_1, option_1, parameter_1;
    const matchValue = Lab_validate(scenario, patch);
    if (matchValue.tag === 0) {
        const forkParameters = matchValue.fields[0];
        const baseline = Lab_evaluate(scenario, Lab_defaults(scenario));
        const fork = Lab_evaluate(scenario, forkParameters);
        return new FSharpResult$2(/* Ok */ 0, [new LabReport(new ExperimentComparison(baseline, fork, map_3((key, value) => ((find(key, fork.Metrics) - value) | 0), baseline.Metrics)), (option_3 = sweepParameter, (option_3 != null) ? ((key_1 = option_3, (option_1 = tryFind((parameter) => (parameter.Key === key_1), scenario.Parameters), (option_1 != null) ? ((parameter_1 = option_1, new SweepResult(key_1, map_1((value_1) => Lab_evaluate(scenario, add(key_1, value_1, forkParameters)), toList_1(rangeDouble(parameter_1.Minimum, parameter_1.Step, parameter_1.Maximum)))))) : undefined))) : undefined), "Exploratory balance evidence — not accepted balance")]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [matchValue.fields[0]]);
    }
}

export function Lab_attackFrames(report) {
    const parameters = report.Comparison.Fork.Input.Parameters;
    const attackPower = Lab_required("attack-power", parameters) | 0;
    return map_1((attack) => [attack, max(0, 100 - (attack * attackPower))], toList_1(rangeDouble(0, 1, Lab_required("attack-count", parameters))));
}

/**
 * Adapts the disclosed deterministic laboratory result to a sandbox frame.
 * The fixed cells and edge are facts of the canonical minimal-slice scenario.
 */
export function Lab_renderFrame(result) {
    let extent;
    const option_1 = CellExtentModule_tryCreate(1);
    if (option_1 != null) {
        extent = option_1;
    }
    else {
        throw new Exception("One-cell laboratory extent is invalid.");
    }
    const visualHealth = (value) => {
        const option_3 = HealthVisualModule_tryCreate(value, 100);
        if (option_3 != null) {
            return option_3;
        }
        else {
            throw new Exception("Laboratory health is outside its validated range.");
        }
    };
    const remaining = Lab_required("remaining-health", result.Metrics) | 0;
    const attacks = Lab_required("attack-events", result.Metrics) | 0;
    return new RenderFrame(attacks, new BoardVisual(0, 0, 2, 1), [new UnitVisual(10, 1, 1, extent, extent, UnitClassIdModule_placeholder, FactionVisual.Arcane, new Disclosure$1(/* Disclosed */ 3, [visualHealth(100)]), Disclosure$1.NotPresent, Disclosure$1.NotPresent, Disclosure$1.NotPresent, Disclosure$1.NotPresent, new Disclosure$1(/* Disclosed */ 3, ["10"]), []), new UnitVisual(20, 2, 0, extent, extent, UnitClassIdModule_placeholder, FactionVisual.Human, new Disclosure$1(/* Disclosed */ 3, [visualHealth(remaining)]), Disclosure$1.NotPresent, Disclosure$1.NotPresent, Disclosure$1.NotPresent, Disclosure$1.NotPresent, new Disclosure$1(/* Disclosed */ 3, ["20"]), [])], [new EdgeVisual("minimal-slice-blocking-edge", "wall", "blocking", 1, 0, 2, 0)], [], initialize(attacks, (index) => (new RenderEventVisual(index, index + 1, "derived-attack", new Disclosure$1(/* Disclosed */ 3, [10]), new Disclosure$1(/* Disclosed */ 3, [20]), Disclosure$1.NotPresent))), DisclosureLabel.SandboxDisclosure);
}

function Lab_entriesOf(map) {
    return map_2((tupledArg) => (new Int32Entry(tupledArg[0], tupledArg[1])), toArray(map));
}

function Lab_mapOfEntries(entries) {
    return ofArray_1(map_2((entry) => [entry.Key, entry.Value], entries), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
}

export function Lab_parametersToTransport(parameters) {
    return Lab_entriesOf(parameters);
}

export function Lab_parametersFromTransport(parameters) {
    return Lab_mapOfEntries(parameters);
}

export function Lab_scenarioToTransport(scenario) {
    return new DesignScenarioTransport(scenario.Identity, scenario.Revision, scenario.Title, scenario.Description, scenario.EngineIdentity, scenario.RulesetIdentity, toArray_1(scenario.Parameters));
}

export function Lab_scenarioFromTransport(scenario) {
    return new DesignScenario(scenario.Identity, scenario.Revision, scenario.Title, scenario.Description, scenario.EngineIdentity, scenario.RulesetIdentity, ofArray(scenario.Parameters));
}

function Lab_resultToTransport(result) {
    return new ExperimentResultTransport(new ExperimentInputTransport(result.Input.ScenarioIdentity, result.Input.ScenarioRevision, result.Input.EngineIdentity, result.Input.RulesetIdentity, Lab_entriesOf(result.Input.Parameters)), result.ResultIdentity, Lab_entriesOf(result.Metrics));
}

function Lab_resultFromTransport(result) {
    return new ExperimentResult(new ExperimentInput(result.Input.ScenarioIdentity, result.Input.ScenarioRevision, result.Input.EngineIdentity, result.Input.RulesetIdentity, Lab_mapOfEntries(result.Input.Parameters)), result.ResultIdentity, Lab_mapOfEntries(result.Metrics));
}

export function Lab_reportToTransport(report) {
    let option_1, sweep;
    return new LabReportTransport(Lab_resultToTransport(report.Comparison.Baseline), Lab_resultToTransport(report.Comparison.Fork), Lab_entriesOf(report.Comparison.Delta), (option_1 = report.Sweep, (option_1 != null) ? ((sweep = option_1, new SweepResultTransport(sweep.Parameter, toArray_1(map_1(Lab_resultToTransport, sweep.Results))))) : undefined), report.EvidenceLabel);
}

export function Lab_reportFromTransport(report) {
    let option_1, sweep;
    return new LabReport(new ExperimentComparison(Lab_resultFromTransport(report.Baseline), Lab_resultFromTransport(report.Fork), Lab_mapOfEntries(report.Delta)), (option_1 = report.Sweep, (option_1 != null) ? ((sweep = option_1, new SweepResult(sweep.Parameter, ofArray(map_2(Lab_resultFromTransport, sweep.Results))))) : undefined), report.EvidenceLabel);
}

export function Lab_export(report) {
    let option_1, sweep;
    const resultLines = (prefix, result) => append(ofArray([(prefix + ".result=") + result.ResultIdentity, (prefix + ".scenario=") + result.Input.ScenarioIdentity, (prefix + ".revision=") + int32ToString(result.Input.ScenarioRevision), (prefix + ".engine=") + result.Input.EngineIdentity, (prefix + ".ruleset=") + result.Input.RulesetIdentity]), append(map_1((tupledArg) => ((((prefix + ".parameter.") + tupledArg[0]) + "=") + int32ToString(tupledArg[1])), toList(result.Input.Parameters)), map_1((tupledArg_1) => ((((prefix + ".metric.") + tupledArg_1[0]) + "=") + int32ToString(tupledArg_1[1])), toList(result.Metrics))));
    return join("\n", append(ofArray(["format=sir-lab-experiment-v1", "evidence=" + report.EvidenceLabel]), append(resultLines("baseline", report.Comparison.Baseline), append(resultLines("fork", report.Comparison.Fork), defaultArg((option_1 = report.Sweep, (option_1 != null) ? ((sweep = option_1, append(ofArray(["sweep.parameter=" + sweep.Parameter, "sweep.count=" + int32ToString(length(sweep.Results))]), mapIndexed((index, result_1) => ((((("sweep." + int32ToString(index)) + "=") + result_1.ResultIdentity) + ",") + int32ToString(find(sweep.Parameter, result_1.Input.Parameters))), sweep.Results)))) : undefined), empty()))))) + "\n";
}

