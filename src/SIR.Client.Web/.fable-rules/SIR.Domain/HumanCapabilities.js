
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { array_type, record_type, int32_type, string_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { copy, map, choose, tryFind } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { compare } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { Array_distinct } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";
import { Exception, stringHash } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { Result_DefaultWith, FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { toArray } from "../fable_modules/fable-library-js.5.13.0/Option.js";

/**
 * The targeting shape owned by a versioned capability descriptor.
 */
export class CapabilityTargetContract extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["PointTarget", "AreaTarget"];
    }
    static PointTarget = new CapabilityTargetContract(0, []);
    static AreaTarget = new CapabilityTargetContract(1, []);
}

export function CapabilityTargetContract_$reflection() {
    return union_type("SIR.Domain.CapabilityTargetContract", [], CapabilityTargetContract, () => [[], []]);
}

/**
 * What interruption does to preparation already invested in an action.
 */
export class CapabilityInterruptionRule extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["LosePreparation", "PreservePreparation"];
    }
    static LosePreparation = new CapabilityInterruptionRule(0, []);
    static PreservePreparation = new CapabilityInterruptionRule(1, []);
}

export function CapabilityInterruptionRule_$reflection() {
    return union_type("SIR.Domain.CapabilityInterruptionRule", [], CapabilityInterruptionRule, () => [[], []]);
}

/**
 * Versioned, data-owned semantics for one ordinary human weapon role.
 */
export class HumanWeaponCapabilityDescriptor extends Record {
    constructor(CapabilityId, Version, EquipmentName, Role, TargetContract, PreparationTicks, TraverseTicksPerDirection, MaximumRangeCells, AmmunitionPerResolution, InterruptionRule, PlanningDecision) {
        super();
        this.CapabilityId = CapabilityId;
        this.Version = (Version | 0);
        this.EquipmentName = EquipmentName;
        this.Role = Role;
        this.TargetContract = TargetContract;
        this.PreparationTicks = (PreparationTicks | 0);
        this.TraverseTicksPerDirection = (TraverseTicksPerDirection | 0);
        this.MaximumRangeCells = (MaximumRangeCells | 0);
        this.AmmunitionPerResolution = (AmmunitionPerResolution | 0);
        this.InterruptionRule = InterruptionRule;
        this.PlanningDecision = PlanningDecision;
    }
}

export function HumanWeaponCapabilityDescriptor_$reflection() {
    return record_type("SIR.Domain.HumanWeaponCapabilityDescriptor", [], HumanWeaponCapabilityDescriptor, () => [["CapabilityId", string_type], ["Version", int32_type], ["EquipmentName", string_type], ["Role", string_type], ["TargetContract", CapabilityTargetContract_$reflection()], ["PreparationTicks", int32_type], ["TraverseTicksPerDirection", int32_type], ["MaximumRangeCells", int32_type], ["AmmunitionPerResolution", int32_type], ["InterruptionRule", CapabilityInterruptionRule_$reflection()], ["PlanningDecision", string_type]]);
}

/**
 * Explicit equipment attached to one authored unit.
 */
export class AuthoredUnitLoadout extends Record {
    constructor(UnitId, Role, Equipment, CapabilityIds) {
        super();
        this.UnitId = (UnitId | 0);
        this.Role = Role;
        this.Equipment = Equipment;
        this.CapabilityIds = CapabilityIds;
    }
}

export function AuthoredUnitLoadout_$reflection() {
    return record_type("SIR.Domain.AuthoredUnitLoadout", [], AuthoredUnitLoadout, () => [["UnitId", int32_type], ["Role", string_type], ["Equipment", array_type(string_type)], ["CapabilityIds", array_type(string_type)]]);
}

export const HumanCapabilities_descriptors = [new HumanWeaponCapabilityDescriptor("human.weapon.carbine", 1, "Carbine", "Close assault", CapabilityTargetContract.PointTarget, 2, 1, 6, 1, CapabilityInterruptionRule.LosePreparation, "take a close route to exploit fast preparation"), new HumanWeaponCapabilityDescriptor("human.weapon.rifle", 1, "Rifle", "General purpose", CapabilityTargetContract.PointTarget, 4, 1, 12, 1, CapabilityInterruptionRule.LosePreparation, "hold a flexible mid-range firing position"), new HumanWeaponCapabilityDescriptor("human.weapon.shotgun", 1, "Shotgun", "Doorway dominance", CapabilityTargetContract.PointTarget, 1, 1, 3, 1, CapabilityInterruptionRule.LosePreparation, "occupy an interior or doorway-adjacent cell"), new HumanWeaponCapabilityDescriptor("human.weapon.marksman-rifle", 1, "Marksman rifle", "Precision fire", CapabilityTargetContract.PointTarget, 8, 2, 24, 1, CapabilityInterruptionRule.PreservePreparation, "choose a distant stable sightline before preparing"), new HumanWeaponCapabilityDescriptor("human.weapon.support", 1, "Support weapon", "Area denial", CapabilityTargetContract.AreaTarget, 10, 3, 16, 4, CapabilityInterruptionRule.LosePreparation, "prepare a covered position whose traverse reaches a threatened area"), new HumanWeaponCapabilityDescriptor("human.weapon.grenade-launcher", 1, "Grenade launcher", "Indirect area fire", CapabilityTargetContract.AreaTarget, 6, 2, 14, 1, CapabilityInterruptionRule.LosePreparation, "select an area behind intervening cover and preserve launcher range"), new HumanWeaponCapabilityDescriptor("human.weapon.anti-armor-launcher", 1, "Anti-armor launcher", "Hardened target defeat", CapabilityTargetContract.PointTarget, 12, 2, 18, 1, CapabilityInterruptionRule.LosePreparation, "reserve a long exposed preparation window against a hardened point target")];

export function HumanCapabilities_tryFind(capabilityId) {
    return tryFind((descriptor) => (compare(descriptor.CapabilityId, capabilityId, 4) === 0), HumanCapabilities_descriptors);
}

export function HumanCapabilities_createLoadout(unitId, role, capabilityIds) {
    let array_2;
    const resolved = choose(HumanCapabilities_tryFind, capabilityIds);
    if ((resolved.length !== capabilityIds.length) ? true : (((array_2 = Array_distinct(capabilityIds, {
        Equals: (x, y) => (x === y),
        GetHashCode: (x) => (stringHash(x) | 0),
    }), array_2.length)) !== capabilityIds.length)) {
        return new FSharpResult$2(/* Error */ 1, ["Loadout capabilities must be unique accepted human descriptor identifiers."]);
    }
    else {
        return new FSharpResult$2(/* Ok */ 0, [new AuthoredUnitLoadout(unitId, role, map((_arg) => _arg.EquipmentName, resolved), copy(capabilityIds))]);
    }
}

/**
 * UI default only; authored artifacts retain the resulting explicit loadout.
 */
export function HumanCapabilities_defaultLoadout(unitId, classId) {
    return Result_DefaultWith((message) => {
        throw new Exception(message);
    }, HumanCapabilities_createLoadout(unitId, classId, toArray((classId === "rifleman") ? "human.weapon.rifle" : ((classId === "planning-unit") ? "human.weapon.rifle" : ((classId === "gunner") ? "human.weapon.support" : ((classId === "marksman") ? "human.weapon.marksman-rifle" : ((classId === "engineer") ? "human.weapon.shotgun" : ((classId === "medic") ? "human.weapon.carbine" : ((classId === "signaller") ? "human.weapon.grenade-launcher" : undefined)))))))));
}

