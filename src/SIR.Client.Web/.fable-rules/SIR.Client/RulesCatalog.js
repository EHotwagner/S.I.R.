
import { Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { record_type, string_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { concat, map, ofArray } from "../fable_modules/fable-library-js.5.13.0/List.js";

export class UnitRole extends Record {
    constructor(Name, Faction, Status, Role) {
        super();
        this.Name = Name;
        this.Faction = Faction;
        this.Status = Status;
        this.Role = Role;
    }
}

export function UnitRole_$reflection() {
    return record_type("SIR.Client.UnitRole", [], UnitRole, () => [["Name", string_type], ["Faction", string_type], ["Status", string_type], ["Role", string_type]]);
}

export class BodyProfile extends Record {
    constructor(Name, Status, Health, FrontArmor, FlankArmor, RearArmor, SuppressionResistance, RegenerationPerSecond) {
        super();
        this.Name = Name;
        this.Status = Status;
        this.Health = Health;
        this.FrontArmor = FrontArmor;
        this.FlankArmor = FlankArmor;
        this.RearArmor = RearArmor;
        this.SuppressionResistance = SuppressionResistance;
        this.RegenerationPerSecond = RegenerationPerSecond;
    }
}

export function BodyProfile_$reflection() {
    return record_type("SIR.Client.BodyProfile", [], BodyProfile, () => [["Name", string_type], ["Status", string_type], ["Health", string_type], ["FrontArmor", string_type], ["FlankArmor", string_type], ["RearArmor", string_type], ["SuppressionResistance", string_type], ["RegenerationPerSecond", string_type]]);
}

export class PerkProfile extends Record {
    constructor(Family, Name, TacticalChange) {
        super();
        this.Family = Family;
        this.Name = Name;
        this.TacticalChange = TacticalChange;
    }
}

export function PerkProfile_$reflection() {
    return record_type("SIR.Client.PerkProfile", [], PerkProfile, () => [["Family", string_type], ["Name", string_type], ["TacticalChange", string_type]]);
}

export class WeaponRole extends Record {
    constructor(Name, EngagementShape, Target, TacticalRole) {
        super();
        this.Name = Name;
        this.EngagementShape = EngagementShape;
        this.Target = Target;
        this.TacticalRole = TacticalRole;
    }
}

export function WeaponRole_$reflection() {
    return record_type("SIR.Client.WeaponRole", [], WeaponRole, () => [["Name", string_type], ["EngagementShape", string_type], ["Target", string_type], ["TacticalRole", string_type]]);
}

export class WeaponProfile extends Record {
    constructor(Name, Kind, BaseEngageSeconds, RangeSlope, Exponent, Accuracy, DispersionPerMeter, Damage, Penetration, ShotsPerSecond, EffectDensity, SuppressionPerSecond) {
        super();
        this.Name = Name;
        this.Kind = Kind;
        this.BaseEngageSeconds = BaseEngageSeconds;
        this.RangeSlope = RangeSlope;
        this.Exponent = Exponent;
        this.Accuracy = Accuracy;
        this.DispersionPerMeter = DispersionPerMeter;
        this.Damage = Damage;
        this.Penetration = Penetration;
        this.ShotsPerSecond = ShotsPerSecond;
        this.EffectDensity = EffectDensity;
        this.SuppressionPerSecond = SuppressionPerSecond;
    }
}

export function WeaponProfile_$reflection() {
    return record_type("SIR.Client.WeaponProfile", [], WeaponProfile, () => [["Name", string_type], ["Kind", string_type], ["BaseEngageSeconds", string_type], ["RangeSlope", string_type], ["Exponent", string_type], ["Accuracy", string_type], ["DispersionPerMeter", string_type], ["Damage", string_type], ["Penetration", string_type], ["ShotsPerSecond", string_type], ["EffectDensity", string_type], ["SuppressionPerSecond", string_type]]);
}

export class ArmorProfile extends Record {
    constructor(Name, Coverage, Cost) {
        super();
        this.Name = Name;
        this.Coverage = Coverage;
        this.Cost = Cost;
    }
}

export function ArmorProfile_$reflection() {
    return record_type("SIR.Client.ArmorProfile", [], ArmorProfile, () => [["Name", string_type], ["Coverage", string_type], ["Cost", string_type]]);
}

export class EquipmentGroup extends Record {
    constructor(Faction, Status, Category, Items) {
        super();
        this.Faction = Faction;
        this.Status = Status;
        this.Category = Category;
        this.Items = Items;
    }
}

export function EquipmentGroup_$reflection() {
    return record_type("SIR.Client.EquipmentGroup", [], EquipmentGroup, () => [["Faction", string_type], ["Status", string_type], ["Category", string_type], ["Items", string_type]]);
}

export const RulesCatalog_unitRoles = ofArray([new UnitRole("Rifleman", "Human", "Canonical", "Broad baseline competence and flexible substitution"), new UnitRole("Gunner", "Human", "Canonical", "Sustained area fire, support weapons, and fire discipline"), new UnitRole("Marksman", "Human", "Canonical", "Slow-building precision at range and observation"), new UnitRole("Engineer", "Human", "Canonical", "Breaching, demolition, deployables, and prepared positions"), new UnitRole("Medic", "Human", "Canonical", "Aid, stabilization, and specialist casualty procedures"), new UnitRole("Signaller", "Human", "Canonical", "Communications, EW, relays, direction finding, and drones"), new UnitRole("Goblin", "Arcane", "Proposed", "Scouting, skirmishing, sapping, handling, carrying, and crewing"), new UnitRole("Orc", "Arcane", "Proposed", "Formation fighting, assault, archery, command, and anchor defense"), new UnitRole("Troll", "Arcane", "Proposed", "Heavy assault, mobile cover, destruction, transport, and recovery"), new UnitRole("Senior caster", "Arcane", "Canonical shape", "Scarce leader and decisive magical specialist"), new UnitRole("Magical assistant", "Arcane", "Canonical shape", "Lesser magic, ritual contribution, preparation, and continuity")]);

export const RulesCatalog_bodyProfiles = ofArray([new BodyProfile("Human operative", "Prototype", "12", "0", "0", "0", "1.00", "0"), new BodyProfile("Observation or relay drone", "Prototype", "8", "0", "0", "0", "—", "0"), new BodyProfile("Goblin skirmisher", "Prototype", "35", "8", "4", "2", "0.75", "0"), new BodyProfile("Shielded orc", "Prototype", "100", "38", "16", "10", "1.25", "0"), new BodyProfile("Armored troll", "Prototype", "240", "55", "38", "24", "1.80", "6")]);

function RulesCatalog_perks(family, values) {
    return map((tupledArg) => (new PerkProfile(family, tupledArg[0], tupledArg[1])), values);
}

export const RulesCatalog_perkProfiles = concat([RulesCatalog_perks("Rifleman", ofArray([["Point Man", "Improves the constrained first response while advancing"], ["Bounding Partner", "Restores readiness better after movement under confirmed covering fire"], ["Quiet Advance", "Trades speed for reduced visual and acoustic signature"], ["Cross-Trained", "Reduces, but does not erase, off-class equipment penalties"], ["Local Initiative", "Executes the last received intent better while disconnected"], ["Rear Guard", "Maintains observation and readiness during disengagement"]])), RulesCatalog_perks("Gunner", ofArray([["Traverse Discipline", "Redirects an area engagement while preserving preparation"], ["Beaten Zone", "Chooses narrow/deep or broad/shallow engagement shapes"], ["Walking Fire", "Shifts suppression along a declared movement path"], ["Fire Control", "Avoids wasting ammunition on unsuitable parts of an area"], ["Final Protective Fire", "Prepares an ammunition-expensive close defensive line"], ["Crew Drill", "Benefits from a cooperating ammunition or weapon assistant"]])), RulesCatalog_perks("Marksman", ofArray([["Patient Solution", "Preserves limited targeting progress through a very brief obstruction"], ["Spotter Pair", "Uses explicitly relayed spotter observations for an initial solution"], ["Counter-Observer", "Recognizes optics and evidence of surveillance"], ["Target Discrimination", "Identifies observable equipment and behavior before firing"], ["Cold Position", "Produces less movement evidence in a prepared position"], ["Displacement Drill", "Leaves after firing more efficiently but abandons the solution"]])), RulesCatalog_perks("Engineer", ofArray([["Hasty Breach", "Faster, louder, less-controlled entry"], ["Surgical Breach", "Slower entry with constrained collateral damage"], ["Remote Initiation", "Connects a prepared charge to a physical trigger"], ["Field Fortification", "Places cover and obstacles where terrain permits"], ["Trap Sense", "Recognizes disturbed terrain, mines, and ritual sites"], ["Render Safe", "Dismantles eligible deployables and discovered ritual traps"]])), RulesCatalog_perks("Medic", ofArray([["Triage", "Rapidly assesses several casualties"], ["Under Fire", "Permits limited exposed stabilization at added cost or reduced reliability"], ["Damage Control", "Treats a defined complication beyond ordinary aid"], ["Conservative Medicine", "Saves supplies when time and safety permit"], ["Casualty Movement", "Coordinates carrying or dragging with less disruption"], ["Return to Duty", "Improves limited function after stabilization without removing wounds"]])), RulesCatalog_perks("Signaller", ofArray([["Burst Discipline", "Trades immediacy for shorter, less exposed transmissions"], ["Frequency Agility", "Reconfigures faster after interference without granting immunity"], ["Cross-Cueing", "Correlates legitimate acoustic, thermal, radio, and magical observations"], ["False Traffic", "Makes decoy emissions resemble plausible network behavior"], ["Drone Shepherd", "Gives drones better pre-disconnection contingencies"], ["Relay Architect", "Predicts relay coverage and weak links"], ["Borrowed-Eye Hunter", "Recognizes evidence of active critter attunement"]])), RulesCatalog_perks("Leadership", ofArray([["Clear Intent", "Supplies a richer fallback plan before disconnection"], ["Fire Coordinator", "Establishes confirmed-target or covering-fire instructions"], ["Controlled Succession", "Reduces disruption when command transfers"], ["Emission Discipline", "Sets silent, scheduled, emergency-only, or continuous transmission posture"], ["Steady Withdrawal", "Preserves formation and reporting during disengagement"]]))]);

export const RulesCatalog_weaponRoles = ofArray([new WeaponRole("Carbine", "Short base, moderate rise", "Point", "Close and mid-range assault default"), new WeaponRole("Rifle", "Moderate base, shallow rise", "Point", "General-purpose baseline"), new WeaponRole("Shotgun", "Very short base, steep degradation", "Point", "Interior and doorway dominance; poor across open ground"), new WeaponRole("Marksman rifle", "Long base, nearly flat", "Point", "Punishes sustained exposure at range; weak against peeking"), new WeaponRole("Support weapon", "Prepared and slow to redirect", "Area", "Denies ground, suppresses, and covers movement"), new WeaponRole("Grenade launcher", "Slow projectile", "Area", "Reaches behind or over cover"), new WeaponRole("Anti-armor launcher", "Long base, slow projectile", "Point", "Vehicles, hardened positions, and heavy creatures")]);

export const RulesCatalog_weaponProfiles = ofArray([new WeaponProfile("Carbine", "Point", "0.32", "0.018", "1.15", "0.86", "0.005", "30", "24", "2.60", "1.00", "7"), new WeaponProfile("Rifle", "Point", "0.55", "0.012", "1.10", "0.88", "0.004", "35", "28", "2.20", "1.00", "8"), new WeaponProfile("Shotgun", "Point", "0.18", "0.040", "1.25", "0.82", "0.018", "52", "16", "1.25", "1.00", "12"), new WeaponProfile("Marksman rifle", "Point", "1.25", "0.003", "1.00", "0.94", "0.001", "55", "36", "0.65", "1.00", "5"), new WeaponProfile("Support weapon", "Area", "0.72", "0.006", "1.00", "0.78", "0.006", "24", "24", "7.00", "0.12", "42"), new WeaponProfile("Grenade launcher", "Area", "1.10", "0.010", "1.00", "0.72", "0.008", "70", "25", "0.25", "0.35", "30"), new WeaponProfile("Anti-armor launcher", "Point", "1.50", "0.006", "1.00", "0.76", "0.003", "120", "85", "0.30", "1.00", "18")]);

export const RulesCatalog_armorProfiles = ofArray([new ArmorProfile("None or soft", "Fragmentation protection", "Low weight; scout choice"), new ArmorProfile("Plate carrier", "Front and rear plates with soft flanks", "Moderate weight and capacity cost"), new ArmorProfile("Heavy armor", "Plates plus limb and neck protection", "High weight, capacity, and readiness cost")]);

export const RulesCatalog_equipmentGroups = ofArray([new EquipmentGroup("Human", "Canonical capability", "Weapon packages", "Suppressor; compact optic; magnified optic; thermal weapon sight; bipod or support mount; under-barrel launcher; specialist ammunition"), new EquipmentGroup("Human", "Canonical capability", "Communications", "Personal set; command-net set; directional antenna; burst-transmission unit; deployable relay; relay drone; physical courier package"), new EquipmentGroup("Human", "Canonical capability", "Sensors", "Compact optics; magnified observation optic; thermal imager; acoustic direction finder; magical-signature detector; trip sensor; observation drone"), new EquipmentGroup("Human", "Canonical capability", "Electronic warfare", "Configurable jammer; radio direction finder; decoy emitter"), new EquipmentGroup("Human", "Canonical capability", "Medical", "Individual aid kit; stabilization kit; nanomedical stock; diagnostic sensor; casualty harness or folding litter"), new EquipmentGroup("Human", "Canonical capability", "Engineering", "Breaching and demolition charges; cutting tool; remote initiator; deployable cover; obstacles; mines; sensor stakes; critter-control and ritual-disruption tools"), new EquipmentGroup("Human", "Canonical capability", "Sustainment", "Ammunition; batteries; drone parts; engineering consumables; medical stock; relay components"), new EquipmentGroup("Arcane", "Proposed", "Caster equipment", "Casting focus; memory aid; component satchel; ward tokens; scrying vessel; ritual kit; portal frame; bound standard or anchor stone; ceremonial armor"), new EquipmentGroup("Arcane", "Proposed", "Goblin equipment", "Shortbows; slings; knives; light spears; nets; climbing gear; obscuring pots; traps; critter cages; component panniers"), new EquipmentGroup("Arcane", "Proposed", "Orc equipment", "Shields; spears; polearms; axes; heavy bows; armor; pavises; breaching tools; standards; ritual-site defenses"), new EquipmentGroup("Arcane", "Proposed", "Troll equipment", "Harness armor; massive tools and weapons; throwing loads; cargo and casualty harnesses; portable magical infrastructure")]);

