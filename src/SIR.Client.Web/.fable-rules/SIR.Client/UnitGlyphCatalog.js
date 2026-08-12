
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { bool_type, record_type, array_type, union_type, float64_type, string_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { UnitClassIdModule_value, UnitClassIdModule_placeholder, UnitClassIdModule_resolve, UnitClassId_$reflection } from "./ReplayPresentation.js";
import { tryFind, ofArray } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { map } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";

export class GlyphPrimitive extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["FilledPath", "StrokedPath", "Circle"];
    }
}

export function GlyphPrimitive_$reflection() {
    return union_type("SIR.Client.GlyphPrimitive", [], GlyphPrimitive, () => [[["pathData", string_type]], [["pathData", string_type]], [["centerX", float64_type], ["centerY", float64_type], ["radius", float64_type]]]);
}

export class UnitGlyphDefinition extends Record {
    constructor(Id, Name, Description, TextAlternative, Primitives) {
        super();
        this.Id = Id;
        this.Name = Name;
        this.Description = Description;
        this.TextAlternative = TextAlternative;
        this.Primitives = Primitives;
    }
}

export function UnitGlyphDefinition_$reflection() {
    return record_type("SIR.Client.UnitGlyphDefinition", [], UnitGlyphDefinition, () => [["Id", UnitClassId_$reflection()], ["Name", string_type], ["Description", string_type], ["TextAlternative", string_type], ["Primitives", array_type(GlyphPrimitive_$reflection())]]);
}

function UnitGlyphCatalog_glyph(id, name, description, textAlternative, primitives) {
    return new UnitGlyphDefinition(UnitClassIdModule_resolve(id), name, description, textAlternative, primitives);
}

export const UnitGlyphCatalog_placeholder = new UnitGlyphDefinition(UnitClassIdModule_placeholder, "Unknown unit", "A visible diamond with an inset question-mark-like hook; used safely when a class identifier is unsupported.", "Unknown unit class", [new GlyphPrimitive(/* StrokedPath */ 1, ["M12 2 L22 12 L12 22 L2 12 Z"]), new GlyphPrimitive(/* StrokedPath */ 1, ["M8 8 C8 4 16 4 16 9 C16 12 12 12 12 16"]), new GlyphPrimitive(/* Circle */ 2, [12, 19, 1])]);

export const UnitGlyphCatalog_all = [UnitGlyphCatalog_glyph("rifleman", "Rifleman", "A forward chevron crossed by a rifle line.", "Human rifleman", [new GlyphPrimitive(/* FilledPath */ 0, ["M4 18 L12 4 L20 18 L16 18 L12 11 L8 18 Z"]), new GlyphPrimitive(/* StrokedPath */ 1, ["M5 20 L19 8"])]), UnitGlyphCatalog_glyph("gunner", "Gunner", "A heavy horizontal weapon bar on a bipod.", "Human gunner", [new GlyphPrimitive(/* FilledPath */ 0, ["M3 8 H21 V12 H3 Z"]), new GlyphPrimitive(/* StrokedPath */ 1, ["M8 12 L5 21 M16 12 L19 21"])]), UnitGlyphCatalog_glyph("marksman", "Marksman", "A sight diamond around a central precision point.", "Human marksman", [new GlyphPrimitive(/* StrokedPath */ 1, ["M12 2 L22 12 L12 22 L2 12 Z"]), new GlyphPrimitive(/* Circle */ 2, [12, 12, 2])]), UnitGlyphCatalog_glyph("engineer", "Engineer", "A bridge-like lintel over two supports.", "Human engineer", [new GlyphPrimitive(/* FilledPath */ 0, ["M3 5 H21 V9 H3 Z M5 9 H9 V21 H5 Z M15 9 H19 V21 H15 Z"])]), UnitGlyphCatalog_glyph("medic", "Medic", "Four equal blocks form a medical cross with open corners.", "Human medic", [new GlyphPrimitive(/* FilledPath */ 0, ["M9 3 H15 V9 H21 V15 H15 V21 H9 V15 H3 V9 H9 Z"])]), UnitGlyphCatalog_glyph("signaller", "Signaller", "A mast emits two symmetric signal arcs.", "Human signaller", [new GlyphPrimitive(/* FilledPath */ 0, ["M10 10 H14 V22 H10 Z"]), new GlyphPrimitive(/* StrokedPath */ 1, ["M8 9 C5 6 5 3 7 1 M16 9 C19 6 19 3 17 1 M5 12 C1 8 1 4 3 1 M19 12 C23 8 23 4 21 1"])]), UnitGlyphCatalog_glyph("observation-drone", "Observation drone", "A four-arm airframe surrounding an observation lens.", "Observation drone", [new GlyphPrimitive(/* StrokedPath */ 1, ["M12 12 L4 4 M12 12 L20 4 M12 12 L4 20 M12 12 L20 20"]), new GlyphPrimitive(/* Circle */ 2, [4, 4, 2]), new GlyphPrimitive(/* Circle */ 2, [20, 4, 2]), new GlyphPrimitive(/* Circle */ 2, [4, 20, 2]), new GlyphPrimitive(/* Circle */ 2, [20, 20, 2]), new GlyphPrimitive(/* Circle */ 2, [12, 12, 2])]), UnitGlyphCatalog_glyph("relay-drone", "Relay drone", "A four-arm airframe with a central relay mast.", "Relay drone", [new GlyphPrimitive(/* StrokedPath */ 1, ["M12 12 L4 4 M12 12 L20 4 M12 12 L4 20 M12 12 L20 20 M12 12 V3"]), new GlyphPrimitive(/* Circle */ 2, [4, 4, 2]), new GlyphPrimitive(/* Circle */ 2, [20, 4, 2]), new GlyphPrimitive(/* Circle */ 2, [4, 20, 2]), new GlyphPrimitive(/* Circle */ 2, [20, 20, 2]), new GlyphPrimitive(/* FilledPath */ 0, ["M9 3 L12 0 L15 3 Z"])]), UnitGlyphCatalog_glyph("goblin", "Goblin", "A low triangular head with wide pointed ears.", "Arcane goblin", [new GlyphPrimitive(/* FilledPath */ 0, ["M2 8 L8 10 L12 5 L16 10 L22 8 L18 18 L12 22 L6 18 Z"])]), UnitGlyphCatalog_glyph("orc", "Orc", "A broad shield with two upward tusk cuts.", "Arcane orc", [new GlyphPrimitive(/* FilledPath */ 0, ["M4 3 H20 V13 C20 18 16 21 12 23 C8 21 4 18 4 13 Z"]), new GlyphPrimitive(/* StrokedPath */ 1, ["M8 17 L10 11 M16 17 L14 11"])]), UnitGlyphCatalog_glyph("troll", "Troll", "A massive stepped silhouette with wide shoulders.", "Arcane troll", [new GlyphPrimitive(/* FilledPath */ 0, ["M2 7 H7 V3 H17 V7 H22 V20 H16 V15 H8 V20 H2 Z"])]), UnitGlyphCatalog_glyph("senior-caster", "Senior caster", "A six-rayed focus around a central ring.", "Arcane senior caster", [new GlyphPrimitive(/* StrokedPath */ 1, ["M12 1 V7 M12 17 V23 M1 12 H7 M17 12 H23 M4 4 L8 8 M16 16 L20 20 M20 4 L16 8 M8 16 L4 20"]), new GlyphPrimitive(/* Circle */ 2, [12, 12, 4])]), UnitGlyphCatalog_glyph("magical-assistant", "Magical assistant", "A three-rayed focus around a small central ring.", "Arcane magical assistant", [new GlyphPrimitive(/* StrokedPath */ 1, ["M12 2 V8 M3 19 L9 15 M21 19 L15 15"]), new GlyphPrimitive(/* Circle */ 2, [12, 12, 3])]), UnitGlyphCatalog_glyph("ambient-critter", "Ambient critter", "A small body with two distinct tracks.", "Ambient critter", [new GlyphPrimitive(/* FilledPath */ 0, ["M7 10 C7 5 17 5 17 12 C17 17 12 20 7 17 Z"]), new GlyphPrimitive(/* Circle */ 2, [6, 5, 2]), new GlyphPrimitive(/* Circle */ 2, [18, 19, 2])])];

const UnitGlyphCatalog_byId = ofArray(map((definition) => [UnitClassIdModule_value(definition.Id), definition], UnitGlyphCatalog_all), {
    Compare: (x, y) => (comparePrimitives(x, y) | 0),
});

/**
 * Unknown replay input always resolves to the visible placeholder.
 */
export function UnitGlyphCatalog_resolve(classId) {
    return defaultArg(tryFind(UnitClassIdModule_value(classId), UnitGlyphCatalog_byId), UnitGlyphCatalog_placeholder);
}

export class PaletteTokens extends Record {
    constructor(Id, Canvas, Terrain, Grid, Text$, HumanFaction, ArcaneFaction, NeutralFaction, HealthActive, HealthDepleted, Focus, OverlayPatterns, UsesPatterns) {
        super();
        this.Id = Id;
        this.Canvas = Canvas;
        this.Terrain = Terrain;
        this.Grid = Grid;
        this.Text = Text$;
        this.HumanFaction = HumanFaction;
        this.ArcaneFaction = ArcaneFaction;
        this.NeutralFaction = NeutralFaction;
        this.HealthActive = HealthActive;
        this.HealthDepleted = HealthDepleted;
        this.Focus = Focus;
        this.OverlayPatterns = OverlayPatterns;
        this.UsesPatterns = UsesPatterns;
    }
}

export function PaletteTokens_$reflection() {
    return record_type("SIR.Client.PaletteTokens", [], PaletteTokens, () => [["Id", string_type], ["Canvas", string_type], ["Terrain", string_type], ["Grid", string_type], ["Text", string_type], ["HumanFaction", string_type], ["ArcaneFaction", string_type], ["NeutralFaction", string_type], ["HealthActive", string_type], ["HealthDepleted", string_type], ["Focus", string_type], ["OverlayPatterns", array_type(string_type)], ["UsesPatterns", bool_type]]);
}

export const ReplayPalettes_accessibleDefault = new PaletteTokens("accessible-default", "#10161d", "#28343d", "#71808b", "#f7f9fa", "#53b7ff", "#d792ff", "#ffd166", "#ff6b6b", "#59636b", "#ffffff", ["solid", "dash", "dot", "crosshatch"], true);

export const ReplayPalettes_highContrast = new PaletteTokens("high-contrast", "#000000", "#000000", "#ffffff", "#ffffff", "#00ffff", "#ff66ff", "#ffff00", "#ffffff", "#555555", "#00ff00", ["solid", "long-dash", "dense-dot", "crosshatch"], true);

export const ReplayPalettes_monochromePattern = new PaletteTokens("monochrome-pattern", "#ffffff", "#eeeeee", "#555555", "#000000", "#000000", "#000000", "#000000", "#000000", "#b5b5b5", "#000000", ["horizontal", "diagonal", "vertical", "crosshatch"], true);

export const ReplayPalettes_all = [ReplayPalettes_accessibleDefault, ReplayPalettes_highContrast, ReplayPalettes_monochromePattern];

