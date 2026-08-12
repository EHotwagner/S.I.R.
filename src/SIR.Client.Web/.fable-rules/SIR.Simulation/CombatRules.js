
import { Union, Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { Cell_$reflection } from "../fable_modules/FS.GG.Game.Core.0.13.0/Primitives.fs.js";
import { union_type, array_type, uint8_type, record_type, string_type, int32_type, lambda_type, bool_type, list_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { FixedPointModule_raw, FixedPointModule_zero, FixedPointModule_fromRatio, FixedPoint_$reflection } from "../SIR.Domain/FixedPoint.js";
import { RuleApplication, RuleIdModule_value, TransitionContract, AlgorithmContract, FormulaExpr, RuleDefinition, RuleSemantics, RuleKind, RuleMetadata, RuleStatus, ControlledStatement, SourceRef, TypedValue, RuleValue, RuleValueKind, RuleIdModule_create, RulePackageIdentity_$reflection, RuleApplication_$reflection } from "../SIR.Domain/RuleTypes.js";
import { FSharpResult$2, Result_MapError, Result_DefaultWith } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { comparePrimitives, Exception } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { isEmpty, filter, length, find, tryFind, ofArray, map, singleton, empty } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { toText, printf, toFail } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { evaluate, coverageJson, manifestJson, packageIdentity, validate } from "../SIR.Domain/Rules.js";
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";
import { equalsWith } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { LineMode, Los_lineOfSightBy } from "../fable_modules/FS.GG.Game.Core.0.13.0/Los.fs.js";
import { ofList } from "../fable_modules/fable-library-js.5.13.0/Map.js";

export class CombatAttackInput extends Record {
    constructor(Attacker, TargetFootprint, IsTransparent, RangeCells, Suppression, BaseDamage, ArmorRetention, EventId) {
        super();
        this.Attacker = Attacker;
        this.TargetFootprint = TargetFootprint;
        this.IsTransparent = IsTransparent;
        this.RangeCells = (RangeCells | 0);
        this.Suppression = Suppression;
        this.BaseDamage = BaseDamage;
        this.ArmorRetention = ArmorRetention;
        this.EventId = EventId;
    }
}

export function CombatAttackInput_$reflection() {
    return record_type("SIR.Simulation.CombatAttackInput", [], CombatAttackInput, () => [["Attacker", Cell_$reflection()], ["TargetFootprint", list_type(Cell_$reflection())], ["IsTransparent", lambda_type(Cell_$reflection(), bool_type)], ["RangeCells", int32_type], ["Suppression", FixedPoint_$reflection()], ["BaseDamage", FixedPoint_$reflection()], ["ArmorRetention", FixedPoint_$reflection()], ["EventId", string_type]]);
}

export class CombatAttackResult extends Record {
    constructor(Preparation, TraceProbability, ArmorRetention, ExpectedDamage, Explanation) {
        super();
        this.Preparation = Preparation;
        this.TraceProbability = TraceProbability;
        this.ArmorRetention = ArmorRetention;
        this.ExpectedDamage = (ExpectedDamage | 0);
        this.Explanation = Explanation;
    }
}

export function CombatAttackResult_$reflection() {
    return record_type("SIR.Simulation.CombatAttackResult", [], CombatAttackResult, () => [["Preparation", FixedPoint_$reflection()], ["TraceProbability", FixedPoint_$reflection()], ["ArmorRetention", FixedPoint_$reflection()], ["ExpectedDamage", int32_type], ["Explanation", RuleApplication_$reflection()]]);
}

export class RuleReplayBinding extends Record {
    constructor(BoundEngineIdentity, BoundCompatibilityProfile, BoundPackageVersion, BoundSourceCommit, BoundImplementationDigest, BoundSemanticDigest, BoundManifestDigest, BoundExplanation) {
        super();
        this.BoundEngineIdentity = BoundEngineIdentity;
        this.BoundCompatibilityProfile = BoundCompatibilityProfile;
        this.BoundPackageVersion = BoundPackageVersion;
        this.BoundSourceCommit = BoundSourceCommit;
        this.BoundImplementationDigest = BoundImplementationDigest;
        this.BoundSemanticDigest = BoundSemanticDigest;
        this.BoundManifestDigest = BoundManifestDigest;
        this.BoundExplanation = BoundExplanation;
    }
}

export function RuleReplayBinding_$reflection() {
    return record_type("SIR.Simulation.RuleReplayBinding", [], RuleReplayBinding, () => [["BoundEngineIdentity", string_type], ["BoundCompatibilityProfile", string_type], ["BoundPackageVersion", string_type], ["BoundSourceCommit", string_type], ["BoundImplementationDigest", array_type(uint8_type)], ["BoundSemanticDigest", array_type(uint8_type)], ["BoundManifestDigest", array_type(uint8_type)], ["BoundExplanation", RuleApplication_$reflection()]]);
}

export class RetainedRulePackage extends Record {
    constructor(Identity, ManifestJson, CoverageJson) {
        super();
        this.Identity = Identity;
        this.ManifestJson = ManifestJson;
        this.CoverageJson = CoverageJson;
    }
}

export function RetainedRulePackage_$reflection() {
    return record_type("SIR.Simulation.RetainedRulePackage", [], RetainedRulePackage, () => [["Identity", RulePackageIdentity_$reflection()], ["ManifestJson", string_type], ["CoverageJson", string_type]]);
}

export class HistoricalRuleResolution extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ResolvedHistoricalRulePackage", "HistoricalRulePackageUnavailable"];
    }
}

export function HistoricalRuleResolution_$reflection() {
    return union_type("SIR.Simulation.HistoricalRuleResolution", [], HistoricalRuleResolution, () => [[["Item", RetainedRulePackage_$reflection()]], [["manifestDigest", array_type(uint8_type)]]]);
}

function CombatRules_requiredId(value) {
    return Result_DefaultWith((message) => {
        throw new Exception(message);
    }, RuleIdModule_create(value));
}

function CombatRules_fixedValue(unitName, value) {
    return new TypedValue(RuleValueKind.FixedPoint, unitName, new RuleValue(/* FixedPointValue */ 1, [value]));
}

function CombatRules_integerValue(unitName, value) {
    return new TypedValue(RuleValueKind.Integer, unitName, new RuleValue(/* IntegerValue */ 0, [value]));
}

function CombatRules_source(symbol) {
    return new SourceRef(symbol, "src/SIR.Simulation/CombatRules.fs", "712636c2deb4222b6249abd0a89b82ae7e729c11");
}

function CombatRules_statement(trigger, response) {
    return new ControlledStatement(empty(), trigger, "S.I.R. combat simulation", singleton(response));
}

function CombatRules_metadata(id, title, kind, rationale, dependencies, symbol, evidence) {
    return new RuleMetadata(CombatRules_requiredId(id), title, RuleStatus.Canonical, kind, CombatRules_statement(undefined, title), rationale, map(CombatRules_requiredId, dependencies), empty(), CombatRules_source(symbol), singleton("tests/SIR.Conformance.Shared/RulesCorpusFixtures.fs"), singleton("deterministic .NET/Fable canonical equality"), singleton(evidence));
}

function CombatRules_fp(numerator, denominator) {
    return Result_DefaultWith((_arg) => {
        throw new Exception("Invalid combat constant.");
    }, FixedPointModule_fromRatio(numerator, denominator));
}

const CombatRules_one = CombatRules_fp(1, 1);

const CombatRules_zero = FixedPointModule_zero;

const CombatRules_rangeSlope = CombatRules_fp(1, 10);

const CombatRules_weapon = new RuleDefinition(CombatRules_metadata("CONTENT-WEAPON-RIFLE-001", "Representative rifle damage", RuleKind.Fact, "The representative rifle anchors the first executable combat slice.", empty(), "CombatRules.weapon", "rules-corpus-v1"), new RuleSemantics(/* FactSemantics */ 0, [CombatRules_fixedValue("damage", CombatRules_fp(25, 1))]));

const CombatRules_body = new RuleDefinition(CombatRules_metadata("CONTENT-BODY-HUMAN-001", "Representative human armor retention", RuleKind.Fact, "The representative body retains the full effect when no armor is declared.", empty(), "CombatRules.body", "rules-corpus-v1"), new RuleSemantics(/* FactSemantics */ 0, [CombatRules_fixedValue("ratio", CombatRules_one)]));

const CombatRules_engagement = (() => {
    const expression = new FormulaExpr(/* Add */ 2, [new FormulaExpr(/* Constant */ 0, [CombatRules_fixedValue("seconds", CombatRules_one)]), new FormulaExpr(/* Multiply */ 4, [new FormulaExpr(/* Input */ 1, ["range", RuleValueKind.FixedPoint, "seconds"]), new FormulaExpr(/* Constant */ 0, [CombatRules_fixedValue("ratio", CombatRules_rangeSlope)])])]);
    return new RuleDefinition(CombatRules_metadata("COMBAT-ENGAGEMENT-001", "Engagement preparation", RuleKind.Formula, "Preparation grows deterministically with engagement range.", empty(), "CombatRules.engagement", "rules-corpus-v1"), new RuleSemantics(/* FormulaSemantics */ 2, [RuleValueKind.FixedPoint, "seconds", expression]));
})();

const CombatRules_trace = new RuleDefinition(CombatRules_metadata("COMBAT-TRACE-002", "Exposed-footprint trace probability", RuleKind.Algorithm, "Visible target footprint samples produce the explainable trace probability.", empty(), "CombatRules.traceProbability", "fs-gg-game-core-fable-lockstep-v1"), new RuleSemantics(/* AlgorithmSemantics */ 4, [new AlgorithmContract("FS.GG.Game.Core.Los.lineOfSightBy", "FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover", ofArray([["visible", RuleValueKind.Integer, "samples"], ["total", RuleValueKind.Integer, "samples"]]), RuleValueKind.FixedPoint, "ratio", ofArray(["visibleSamples", "totalSamples", "lineMode"]))]));

const CombatRules_armor = new RuleDefinition(CombatRules_metadata("COMBAT-ARMOR-004", "Armor retained effect", RuleKind.Formula, "Armor retention is an explicit bounded ratio.", empty(), "CombatRules.armor", "rules-corpus-v1"), new RuleSemantics(/* FormulaSemantics */ 2, [RuleValueKind.FixedPoint, "ratio", new FormulaExpr(/* Clamp */ 8, [new FormulaExpr(/* Constant */ 0, [CombatRules_fixedValue("ratio", CombatRules_zero)]), new FormulaExpr(/* Constant */ 0, [CombatRules_fixedValue("ratio", CombatRules_one)]), new FormulaExpr(/* Input */ 1, ["retention", RuleValueKind.FixedPoint, "ratio"])])]));

const CombatRules_damage = new RuleDefinition(CombatRules_metadata("COMBAT-DAMAGE-001", "Expected damage", RuleKind.Formula, "Expected damage is the weapon effect multiplied once by trace probability and retained armor effect.", ofArray(["CONTENT-WEAPON-RIFLE-001", "COMBAT-TRACE-002", "COMBAT-ARMOR-004"]), "CombatRules.damage", "rules-corpus-v1"), new RuleSemantics(/* FormulaSemantics */ 2, [RuleValueKind.FixedPoint, "damage", new FormulaExpr(/* Multiply */ 4, [new FormulaExpr(/* Multiply */ 4, [new FormulaExpr(/* Input */ 1, ["baseDamage", RuleValueKind.FixedPoint, "damage"]), new FormulaExpr(/* Input */ 1, ["trace", RuleValueKind.FixedPoint, "ratio"])]), new FormulaExpr(/* Input */ 1, ["retention", RuleValueKind.FixedPoint, "ratio"])])]));

const CombatRules_transition = new RuleDefinition(CombatRules_metadata("COMBAT-ATTACK-RESOLUTION-001", "Resolve one explained attack", RuleKind.Transition, "The attack transition exposes its ordered rule calls and authoritative event.", ofArray(["COMBAT-ENGAGEMENT-001", "COMBAT-TRACE-002", "COMBAT-ARMOR-004", "COMBAT-DAMAGE-001"]), "CombatRules.resolveAttack", "rules-corpus-v1"), new RuleSemantics(/* TransitionSemantics */ 3, [new TransitionContract("AttackPhase", empty(), ofArray(["attacker.cell", "target.footprint", "weapon", "armor"]), singleton("target.health"), singleton("AttackResolved"))]));

export const CombatRules_registry = Result_DefaultWith((errors) => toFail(printf("Invalid combat registry: %A"))(errors), validate(ofArray([CombatRules_weapon, CombatRules_body, CombatRules_engagement, CombatRules_trace, CombatRules_armor, CombatRules_damage, CombatRules_transition])));

export const CombatRules_implementationArtifacts = ofArray([["combat-rules-source-sha256", get_UTF8().getBytes("f6fdd482e35eacfb53759ac0d58f12fdb8924dfc691dcb6b686bf5e9e2556a9d")], ["source-sha256:src/SIR.Domain/Rules.fs", get_UTF8().getBytes("6d344d1833e111f170eb3ee20ec8cdf1e5ba7a067a6dd40831cee140f27834e6")], ["source-sha256:src/SIR.Domain/FixedPoint.fs", get_UTF8().getBytes("fc163a6b8ee048b35b608fbd42a8563df79728d22f479435fc465615d7453b82")], ["source-sha256:src/SIR.Domain/CanonicalEncoding.fs", get_UTF8().getBytes("53336b1aae67c94bfea21bd0cd9ba40fa239b5eaf848b88cbc2a086215d89c73")], ["source-sha256:src/SIR.Domain/CanonicalHash.fs", get_UTF8().getBytes("ebe99fceebbc255f62bf7b8a6c0fff26a96eab0ce2adceef497959d620988f8e")], ["source-sha256:src/SIR.Simulation/Replay.fs", get_UTF8().getBytes("827c569c076f024a5c56fccc18a2ff1f13e10d60544edfc11b0f6799a19e5067")], ["source-sha256:src/SIR.Simulation/Simulation.fs", get_UTF8().getBytes("325b6aaa37921490291bcb2cfe9a17e3e0ef84dc8632acda660699d931b24c88")], ["source-sha256:src/SIR.Match/MatchReplay.fs", get_UTF8().getBytes("727d795e805b028b9cbfe2669cdec8f60badbbc51605b55d4ebc7455660e2fcb")], ["fs-gg-game-core-nupkg-sha256", get_UTF8().getBytes("2722ec4828960167da8e77c2699b0d0a679cd4791207d2bf6f3b644a2bab66f7")], ["los-line-of-sight-by-fingerprint", get_UTF8().getBytes("FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover")]]);

export const CombatRules_packageIdentity = packageIdentity("sir-simulation-v1", "fs-gg-game-core-fable-lockstep-v1", "FS.GG.Game.Core@0.13.0", "712636c2deb4222b6249abd0a89b82ae7e729c11", CombatRules_implementationArtifacts, CombatRules_registry);

export const CombatRules_retainedPackage = new RetainedRulePackage(CombatRules_packageIdentity, manifestJson(CombatRules_packageIdentity, CombatRules_registry), coverageJson(CombatRules_packageIdentity, CombatRules_registry));

export function CombatRules_replayBinding(explanation) {
    return new RuleReplayBinding(CombatRules_packageIdentity.EngineIdentity, CombatRules_packageIdentity.CompatibilityProfile, CombatRules_packageIdentity.PackageVersion, CombatRules_packageIdentity.SourceCommit, CombatRules_packageIdentity.ImplementationDigest, CombatRules_packageIdentity.SemanticDigest, CombatRules_packageIdentity.ManifestDigest, explanation);
}

export function CombatRules_resolveHistoricalPackage(retained, binding) {
    const _arg = tryFind((package$) => {
        const identity = package$.Identity;
        if ((((((identity.EngineIdentity === binding.BoundEngineIdentity) && (identity.CompatibilityProfile === binding.BoundCompatibilityProfile)) && (identity.PackageVersion === binding.BoundPackageVersion)) && (identity.SourceCommit === binding.BoundSourceCommit)) && equalsWith((x, y) => (x === y), identity.ImplementationDigest, binding.BoundImplementationDigest)) && equalsWith((x_1, y_1) => (x_1 === y_1), identity.SemanticDigest, binding.BoundSemanticDigest)) {
            return equalsWith((x_2, y_2) => (x_2 === y_2), identity.ManifestDigest, binding.BoundManifestDigest);
        }
        else {
            return false;
        }
    }, retained);
    if (_arg == null) {
        return new HistoricalRuleResolution(/* HistoricalRulePackageUnavailable */ 1, [binding.BoundManifestDigest]);
    }
    else {
        return new HistoricalRuleResolution(/* ResolvedHistoricalRulePackage */ 0, [_arg]);
    }
}

function CombatRules_evaluate(expression, inputs) {
    let clo;
    const result = evaluate(inputs, expression);
    return Result_MapError((clo = toText(printf("%A")), clo), result);
}

function CombatRules_formula(id) {
    const matchValue = find((rule) => (RuleIdModule_value(rule.Metadata.Id) === id), CombatRules_registry).Semantics;
    if (matchValue.tag === 2) {
        return matchValue.fields[2];
    }
    else {
        throw new Exception("Expected formula.");
    }
}

function CombatRules_application(id, eventId, operands, outcome, children) {
    return new RuleApplication((eventId + ":") + id, CombatRules_requiredId(id), operands, outcome, children, eventId, CombatRules_packageIdentity.ManifestDigest);
}

function CombatRules_traceProbability(attacker, footprint, isTransparent) {
    return [length(filter((b) => Los_lineOfSightBy(LineMode.Supercover, isTransparent, attacker, b), footprint)), length(footprint)];
}

export function CombatRules_resolveAttack(input) {
    if (isEmpty(input.TargetFootprint)) {
        return new FSharpResult$2(/* Error */ 1, ["Target footprint must contain at least one sample."]);
    }
    else {
        const patternInput = CombatRules_traceProbability(input.Attacker, input.TargetFootprint, input.IsTransparent);
        const visible = patternInput[0] | 0;
        const total = patternInput[1] | 0;
        const traceValue = Result_DefaultWith((_arg) => {
            throw new Exception("Non-empty footprint division failed.");
        }, FixedPointModule_fromRatio(visible, total));
        const preparationInputs = ofList(singleton(["range", CombatRules_fixedValue("seconds", CombatRules_fp(input.RangeCells, 1))]), {
            Compare: (x, y) => (comparePrimitives(x, y) | 0),
        });
        const armorInputs = ofList(singleton(["retention", CombatRules_fixedValue("ratio", input.ArmorRetention)]), {
            Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
        });
        const fixedOf = (typed) => {
            const matchValue = typed.Value;
            if (matchValue.tag === 1) {
                return matchValue.fields[0];
            }
            else {
                throw new Exception("Validated formula returned another kind.");
            }
        };
        const matchValue_1 = CombatRules_evaluate(CombatRules_formula("COMBAT-ENGAGEMENT-001"), preparationInputs);
        const matchValue_2 = CombatRules_evaluate(CombatRules_formula("COMBAT-ARMOR-004"), armorInputs);
        let matchResult, preparation, retained, error_1;
        const copyOfStruct = matchValue_1;
        if (copyOfStruct.tag === 1) {
            matchResult = 1;
            error_1 = copyOfStruct.fields[0];
        }
        else {
            const copyOfStruct_1 = matchValue_2;
            if (copyOfStruct_1.tag === 1) {
                matchResult = 1;
                error_1 = copyOfStruct_1.fields[0];
            }
            else {
                matchResult = 0;
                preparation = copyOfStruct.fields[0];
                retained = copyOfStruct_1.fields[0];
            }
        }
        switch (matchResult) {
            case 0: {
                const damageInputs = ofList(ofArray([["baseDamage", CombatRules_fixedValue("damage", input.BaseDamage)], ["trace", CombatRules_fixedValue("ratio", traceValue)], ["retention", retained]]), {
                    Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
                });
                const matchValue_4 = CombatRules_evaluate(CombatRules_formula("COMBAT-DAMAGE-001"), damageInputs);
                if (matchValue_4.tag === 0) {
                    const expected = matchValue_4.fields[0];
                    const roundedDamage = ~~((FixedPointModule_raw(fixedOf(expected)) + ~~(10000 / 2)) / 10000) | 0;
                    const traceTyped = CombatRules_fixedValue("ratio", traceValue);
                    const children = ofArray([CombatRules_application("COMBAT-ENGAGEMENT-001", input.EventId, ofArray([["rangeCells", CombatRules_integerValue("cells", input.RangeCells)], ["suppression", CombatRules_fixedValue("suppression", input.Suppression)]]), preparation, empty()), CombatRules_application("COMBAT-TRACE-002", input.EventId, ofArray([["visibleSamples", CombatRules_integerValue("samples", visible)], ["totalSamples", CombatRules_integerValue("samples", total)], ["lineMode", new TypedValue(RuleValueKind.Text$, "name", new RuleValue(/* TextValue */ 3, ["Supercover"]))]]), traceTyped, empty()), CombatRules_application("COMBAT-ARMOR-004", input.EventId, singleton(["retention", CombatRules_fixedValue("ratio", input.ArmorRetention)]), retained, empty()), CombatRules_application("COMBAT-DAMAGE-001", input.EventId, ofArray([["baseDamage", CombatRules_fixedValue("damage", input.BaseDamage)], ["trace", traceTyped], ["retention", retained]]), expected, empty())]);
                    const outcome = CombatRules_integerValue("damage", roundedDamage);
                    return new FSharpResult$2(/* Ok */ 0, [new CombatAttackResult(fixedOf(preparation), traceValue, fixedOf(retained), roundedDamage, CombatRules_application("COMBAT-ATTACK-RESOLUTION-001", input.EventId, empty(), outcome, children))]);
                }
                else {
                    return new FSharpResult$2(/* Error */ 1, [matchValue_4.fields[0]]);
                }
            }
            default:
                return new FSharpResult$2(/* Error */ 1, [error_1]);
        }
    }
}

