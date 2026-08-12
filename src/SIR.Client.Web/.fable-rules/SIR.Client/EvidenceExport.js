
import { toString, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { record_type, int32_type, option_type, string_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { indexOf, join as join_1, substring, replace, format } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { singleton, append as append_1, delay, map, toArray } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { isLetterOrDigit } from "../fable_modules/fable-library-js.5.13.0/Char.js";
import { max, min } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { item, tryFind, sortBy, sort, initialize, map as map_1 } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { byteValue, int32LittleEndian, concatenate } from "../SIR.Domain/CanonicalEncoding.js";
import { doubleToInt64Bits } from "../fable_modules/fable-library-js.5.13.0/BitConverter.js";
import { op_RightShift, toInt64_unchecked, toUInt8_unchecked } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";
import { sha256 } from "../SIR.Domain/CanonicalHash.js";
import { HeadingRadiansModule_value, HealthVisualModule_maximum, HealthVisualModule_remaining, UnitClassIdModule_value, CellExtentModule_value } from "./ReplayPresentation.js";
import { int32ToString, compareArrays, comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { ReplayPalettes_accessibleDefault, ReplayPalettes_all } from "./UnitGlyphCatalog.js";
import { BattlefieldViewState, Battlefield_initial, Battlefield_scene } from "./Battlefield.js";
import { StringBuilder__Append_Z721C83C5, StringBuilder_$ctor_Z524259A4 } from "../fable_modules/fable-library-js.5.13.0/System.Text.js";
import { forAll, ofArray } from "../fable_modules/fable-library-js.5.13.0/List.js";

export class EvidenceMode extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["VerifiedReplayEvidence", "PerspectiveEvidence", "DerivedSimulationEvidence"];
    }
    static VerifiedReplayEvidence = new EvidenceMode(0, []);
    static PerspectiveEvidence = new EvidenceMode(1, []);
    static DerivedSimulationEvidence = new EvidenceMode(2, []);
}

export function EvidenceMode_$reflection() {
    return union_type("SIR.Client.EvidenceMode", [], EvidenceMode, () => [[], [], []]);
}

export class EvidenceProvenance extends Record {
    constructor(SourceIdentity, ReplayIdentity, ProjectionIdentity, EngineIdentity, RulesetIdentity, Tick, Mode, PaletteIdentity, RendererVersion) {
        super();
        this.SourceIdentity = SourceIdentity;
        this.ReplayIdentity = ReplayIdentity;
        this.ProjectionIdentity = ProjectionIdentity;
        this.EngineIdentity = EngineIdentity;
        this.RulesetIdentity = RulesetIdentity;
        this.Tick = (Tick | 0);
        this.Mode = Mode;
        this.PaletteIdentity = PaletteIdentity;
        this.RendererVersion = RendererVersion;
    }
}

export function EvidenceProvenance_$reflection() {
    return record_type("SIR.Client.EvidenceProvenance", [], EvidenceProvenance, () => [["SourceIdentity", string_type], ["ReplayIdentity", string_type], ["ProjectionIdentity", string_type], ["EngineIdentity", string_type], ["RulesetIdentity", option_type(string_type)], ["Tick", int32_type], ["Mode", EvidenceMode_$reflection()], ["PaletteIdentity", string_type], ["RendererVersion", string_type]]);
}

export class SvgEvidence extends Record {
    constructor(FileName, MediaType, Svg, Sha256, Provenance) {
        super();
        this.FileName = FileName;
        this.MediaType = MediaType;
        this.Svg = Svg;
        this.Sha256 = Sha256;
        this.Provenance = Provenance;
    }
}

export function SvgEvidence_$reflection() {
    return record_type("SIR.Client.SvgEvidence", [], SvgEvidence, () => [["FileName", string_type], ["MediaType", string_type], ["Svg", string_type], ["Sha256", string_type], ["Provenance", EvidenceProvenance_$reflection()]]);
}

function EvidenceExport_invariant(value) {
    return format('{0:' + "0.###" + '}', value);
}

function EvidenceExport_escapeText(value) {
    return replace(replace(replace(replace(replace(value, "&", "&amp;"), "<", "&lt;"), ">", "&gt;"), "\"", "&quot;"), "\'", "&apos;");
}

function EvidenceExport_boundedText(maximum, value) {
    let value_1;
    return EvidenceExport_escapeText((value_1 = toArray(map((character) => {
        if ((((isLetterOrDigit(character) ? true : (character === " ")) ? true : (character === "-")) ? true : (character === "_")) ? true : (character === ".")) {
            return character;
        }
        else {
            return " ";
        }
    }, substring(value, 0, min(maximum, value.length)).split(""))), value_1.join('')));
}

function EvidenceExport_hex(bytes) {
    return join_1("", map_1((value) => format('{0:' + "x2" + '}', value), bytes));
}

export function EvidenceExport_projectionIdentity(frame) {
    let matchValue_2;
    const join = concatenate;
    const int32Bytes = int32LittleEndian;
    const tag = byteValue;
    const floatBytes = (value_3) => {
        const value_2 = doubleToInt64Bits(value_3);
        return initialize(8, (shift) => (toUInt8_unchecked(toInt64_unchecked(op_RightShift(value_2, shift * 8))) & 0xFF), Uint8Array);
    };
    const stringBytes = (value_5) => {
        const bytes = get_UTF8().getBytes(value_5);
        return join([int32Bytes(bytes.length), bytes]);
    };
    const arrayBytes = (encode, values) => join(delay(() => append_1(singleton(int32Bytes(values.length)), delay(() => map(encode, values)))));
    const disclosureBytes = (encode_1, value_7) => {
        switch (value_7.tag) {
            case 1:
                return tag(1);
            case 2:
                return tag(2);
            case 3:
                return join([tag(3), encode_1(value_7.fields[0])]);
            default:
                return tag(0);
        }
    };
    return EvidenceExport_hex(sha256(join([stringBytes("sir-render-projection-v1"), int32Bytes(frame.Tick), int32Bytes(frame.Board.MinimumColumn), int32Bytes(frame.Board.MinimumRow), int32Bytes(frame.Board.MaximumColumn), int32Bytes(frame.Board.MaximumRow), arrayBytes((unit) => {
        let value_8;
        return join([int32Bytes(unit.Id), int32Bytes(unit.AnchorColumn), int32Bytes(unit.AnchorRow), int32Bytes(CellExtentModule_value(unit.FootprintWidth)), int32Bytes(CellExtentModule_value(unit.FootprintDepth)), stringBytes(UnitClassIdModule_value(unit.ClassId)), (value_8 = unit.Faction, (value_8.tag === 1) ? tag(1) : ((value_8.tag === 2) ? tag(2) : ((value_8.tag === 3) ? join([tag(3), stringBytes(value_8.fields[0])]) : tag(0)))), disclosureBytes((health) => join([int32Bytes(HealthVisualModule_remaining(health)), int32Bytes(HealthVisualModule_maximum(health))]), unit.Health), disclosureBytes(int32Bytes, unit.Level), disclosureBytes(stringBytes, unit.StanceId), disclosureBytes((arg_6) => floatBytes(HeadingRadiansModule_value(arg_6)), unit.BodyHeading), disclosureBytes((secondary) => {
            let matchValue;
            return join([tag((matchValue = secondary.Source, (matchValue.tag === 2) ? 1 : ((matchValue.tag === 0) ? 2 : 0))), floatBytes(HeadingRadiansModule_value(secondary.Radians))]);
        }, unit.SecondaryHeading), disclosureBytes(stringBytes, unit.ShortLabel), arrayBytes(stringBytes, sort(unit.StatusIds, {
            Compare: (x, y) => (comparePrimitives(x, y) | 0),
        }))]);
    }, sortBy((_arg) => (_arg.Id | 0), frame.Units, {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    })), arrayBytes((edge) => join([stringBytes(edge.Id), stringBytes(edge.Kind), stringBytes(edge.State), int32Bytes(edge.StartColumn), int32Bytes(edge.StartRow), int32Bytes(edge.EndColumn), int32Bytes(edge.EndRow)]), sortBy((_arg_1) => _arg_1.Id, frame.Edges, {
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    })), arrayBytes((overlay) => {
        let matchValue_1;
        return join([stringBytes(overlay.Id), stringBytes(overlay.Kind), (matchValue_1 = overlay.Scope, (matchValue_1.tag === 1) ? tag(1) : join([tag(0), int32Bytes(matchValue_1.fields[0])])), int32Bytes(overlay.GeometryRevision), arrayBytes(floatBytes, overlay.Points), disclosureBytes(stringBytes, overlay.Label)]);
    }, sortBy((_arg_2) => _arg_2.Id, frame.Overlays, {
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    })), arrayBytes((event) => join([int32Bytes(event.Id), int32Bytes(event.Tick), stringBytes(event.Kind), disclosureBytes(int32Bytes, event.SourceUnitId), disclosureBytes(int32Bytes, event.TargetUnitId), disclosureBytes(stringBytes, event.Summary)]), sortBy((event_1) => [event_1.Tick, event_1.Id], frame.Events, {
        Compare: (x_4, y_4) => (compareArrays(x_4, y_4) | 0),
    })), tag((matchValue_2 = frame.Disclosure, (matchValue_2.tag === 1) ? 1 : ((matchValue_2.tag === 2) ? 2 : 0)))])));
}

function EvidenceExport_modeText(mode) {
    switch (mode.tag) {
        case 1:
            return "perspective-evidence";
        case 2:
            return "derived-simulation-not-verified";
        default:
            return "verified-replay-evidence";
    }
}

function EvidenceExport_palette(paletteIdentity) {
    return defaultArg(tryFind((candidate) => (candidate.Id === paletteIdentity), ReplayPalettes_all), ReplayPalettes_accessibleDefault);
}

/**
 * Generates a closed, presentation-only SVG. It never serializes DOM or replay markup.
 */
export function EvidenceExport_svg(provenance, annotation, frame) {
    let objectArg;
    const palette = EvidenceExport_palette(provenance.PaletteIdentity);
    const provenance_1 = new EvidenceProvenance(provenance.SourceIdentity, provenance.ReplayIdentity, EvidenceExport_projectionIdentity(frame), provenance.EngineIdentity, provenance.RulesetIdentity, frame.Tick, provenance.Mode, palette.Id, "sir-safe-svg-renderer-v1");
    const scene = Battlefield_scene(frame, new BattlefieldViewState(Battlefield_initial.Camera, Battlefield_initial.SemanticZoom, Battlefield_initial.SelectedUnit, Battlefield_initial.FocusedUnit, palette.Id, true, true));
    const width = max(1, scene.Width);
    const height = max(1, scene.Height) + 86;
    const builder = StringBuilder_$ctor_Z524259A4(4096);
    const append = (text) => {
        StringBuilder__Append_Z721C83C5(builder, text);
    };
    const rect = (x, y, w, h, fill, stroke) => {
        append(((((((((((("<rect x=\"" + EvidenceExport_invariant(x)) + "\" y=\"") + EvidenceExport_invariant(y)) + "\" width=\"") + EvidenceExport_invariant(w)) + "\" height=\"") + EvidenceExport_invariant(h)) + "\" fill=\"") + fill) + "\" stroke=\"") + stroke) + "\"/>");
    };
    const line = (x1, y1, x2, y2, stroke_1, width_1) => {
        append(((((((((((("<line x1=\"" + EvidenceExport_invariant(x1)) + "\" y1=\"") + EvidenceExport_invariant(y1)) + "\" x2=\"") + EvidenceExport_invariant(x2)) + "\" y2=\"") + EvidenceExport_invariant(y2)) + "\" stroke=\"") + stroke_1) + "\" stroke-width=\"") + EvidenceExport_invariant(width_1)) + "\"/>");
    };
    append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
    append(((((((("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"" + EvidenceExport_invariant(width)) + "\" height=\"") + EvidenceExport_invariant(height)) + "\" viewBox=\"0 0 ") + EvidenceExport_invariant(width)) + " ") + EvidenceExport_invariant(height)) + "\" role=\"img\" aria-label=\"SIR evidence export\">");
    append("<metadata>");
    append(("source=" + EvidenceExport_boundedText(256, provenance_1.SourceIdentity)) + "\n");
    append(("replay=" + EvidenceExport_boundedText(256, provenance_1.ReplayIdentity)) + "\n");
    append(("projection=" + EvidenceExport_boundedText(128, provenance_1.ProjectionIdentity)) + "\n");
    append(("engine=" + EvidenceExport_boundedText(128, provenance_1.EngineIdentity)) + "\n");
    append(("ruleset=" + EvidenceExport_boundedText(128, defaultArg(provenance_1.RulesetIdentity, "not-available"))) + "\n");
    append(("tick=" + int32ToString(provenance_1.Tick)) + "\n");
    append(("mode=" + EvidenceExport_modeText(provenance_1.Mode)) + "\n");
    append(("palette=" + EvidenceExport_boundedText(64, palette.Id)) + "\n");
    append("renderer=" + EvidenceExport_boundedText(64, provenance_1.RendererVersion));
    append("</metadata>");
    rect(0, 0, width, scene.Height, palette.Terrain, palette.Grid);
    const columns = ((frame.Board.MaximumColumn - frame.Board.MinimumColumn) + 1) | 0;
    const rows = ((frame.Board.MaximumRow - frame.Board.MinimumRow) + 1) | 0;
    for (let index = 0; index <= columns; index++) {
        line(index * scene.CellSize, 0, index * scene.CellSize, scene.Height, palette.Grid, 1);
    }
    for (let index_1 = 0; index_1 <= rows; index_1++) {
        line(0, index_1 * scene.CellSize, width, index_1 * scene.CellSize, palette.Grid, 1);
    }
    const arr = sortBy((_arg) => _arg.Id, scene.Edges, {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    });
    for (let idx = 0; idx <= (arr.length - 1); idx++) {
        const edge = item(idx, arr);
        line((edge.StartColumn - scene.Board.MinimumColumn) * scene.CellSize, (edge.StartRow - scene.Board.MinimumRow) * scene.CellSize, (edge.EndColumn - scene.Board.MinimumColumn) * scene.CellSize, (edge.EndRow - scene.Board.MinimumRow) * scene.CellSize, palette.Text, 3);
    }
    const arr_1 = sortBy((_arg_1) => (_arg_1.Unit.Id | 0), scene.Units, {
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    });
    for (let idx_1 = 0; idx_1 <= (arr_1.length - 1); idx_1++) {
        const unit = item(idx_1, arr_1);
        let faction;
        const matchValue = unit.Unit.Faction;
        switch (matchValue.tag) {
            case 1: {
                faction = palette.ArcaneFaction;
                break;
            }
            case 2:
            case 3: {
                faction = palette.NeutralFaction;
                break;
            }
            default:
                faction = palette.HumanFaction;
        }
        rect(unit.FootprintX, unit.FootprintY, unit.FootprintWidth, unit.FootprintDepth, "none", faction);
        rect(unit.SymbolCenterX - 14, unit.SymbolCenterY - 14, 28, 28, palette.Canvas, faction);
        append(((((((("<text x=\"" + EvidenceExport_invariant(unit.SymbolCenterX - 10)) + "\" y=\"") + EvidenceExport_invariant(unit.SymbolCenterY + 4)) + "\" fill=\"") + palette.Text) + "\" font-family=\"sans-serif\" font-size=\"10\">") + EvidenceExport_boundedText(12, int32ToString(unit.Unit.Id))) + "</text>");
    }
    rect(0, scene.Height, width, 86, palette.Canvas, palette.Grid);
    let title;
    const matchValue_1 = provenance_1.Mode;
    title = ((matchValue_1.tag === 0) ? "VERIFIED REPLAY EVIDENCE" : ((matchValue_1.tag === 1) ? "PERSPECTIVE EVIDENCE — HIDDEN STATE OMITTED" : "DERIVED SIMULATION — NOT VERIFIED REPLAY"));
    append(((((("<text x=\"8\" y=\"" + EvidenceExport_invariant(scene.Height + 20)) + "\" fill=\"") + palette.Text) + "\" font-family=\"sans-serif\" font-size=\"12\" font-weight=\"bold\">") + title) + "</text>");
    append(((((((((("<text x=\"8\" y=\"" + EvidenceExport_invariant(scene.Height + 40)) + "\" fill=\"") + palette.Text) + "\" font-family=\"monospace\" font-size=\"9\">tick ") + int32ToString(provenance_1.Tick)) + " · projection ") + EvidenceExport_boundedText(20, provenance_1.ProjectionIdentity)) + " · palette ") + EvidenceExport_boundedText(32, palette.Id)) + "</text>");
    const option_2 = annotation;
    if (option_2 != null) {
        const value_2 = option_2;
        append(((((("<text x=\"8\" y=\"" + EvidenceExport_invariant(scene.Height + 60)) + "\" fill=\"") + palette.Text) + "\" font-family=\"sans-serif\" font-size=\"9\">") + EvidenceExport_boundedText(120, value_2)) + "</text>");
    }
    append("</svg>\n");
    const content = toString(builder);
    const sha = EvidenceExport_hex(sha256((objectArg = get_UTF8(), objectArg.getBytes(content))));
    return new SvgEvidence(("sir-evidence-tick-" + int32ToString(provenance_1.Tick)) + ".svg", "image/svg+xml;charset=utf-8", content, sha, provenance_1);
}

export const EvidenceExport_forbiddenTokens = ofArray(["<script", "onload=", "onclick=", "onerror=", "<foreignObject", "href=", "url(", "http://", "https://", "data:", "<style", "<path", " id="]);

export function EvidenceExport_isClosedSvg(content) {
    const contentWithoutNamespace = replace(content, "xmlns=\"http://www.w3.org/2000/svg\"", "", 4);
    return forAll((token) => (indexOf(contentWithoutNamespace, token, 5) < 0), EvidenceExport_forbiddenTokens);
}

