
import { toString, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { array_type, bool_type, string_type, option_type, int32_type, record_type, float64_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { RenderFrame, DisclosureLabel, RenderEventVisual, OverlayVisual, OverlayScope, EdgeVisual, FactionVisual, SecondaryHeadingSource, BoardVisual, SecondaryHeadingVisual, UnitVisual, Disclosure$1, UnitClassIdModule_resolve, HealthVisualModule_tryCreate, HeadingRadiansModule_tryCreate, CellExtentModule_tryCreate, CellExtentModule_value, HeadingRadiansModule_value, HealthVisualModule_maximum, HealthVisualModule_remaining, DisclosureLabel_$reflection, EdgeVisual_$reflection, BoardVisual_$reflection, UnitVisual_$reflection, Disclosure$1_$reflection, OverlayVisual_$reflection } from "./ReplayPresentation.js";
import { UnitGlyphCatalog_resolve, ReplayPalettes_all, ReplayPalettes_accessibleDefault, PaletteTokens_$reflection } from "./UnitGlyphCatalog.js";
import { Exception, compareArrays, sign, comparePrimitives, equals, int32ToString, round, min, compare, max } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { orElse, value as value_4, defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { sortBy, tryHead, sumBy, map, append, choose, fold, partition, max as max_2, min as min_2, initialize, copy, item, tryFind } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { min as min_3, op_Multiply, op_Addition, op_Subtraction, op_Division, toInt32_unchecked, fromInt32, toInt64_unchecked } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";
import { isInfinity, min as min_1, max as max_1 } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { isEmpty, tryFind as tryFind_1, ofArray } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { contains, ofArray as ofArray_1 } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { join, format } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { singleton, append as append_1, delay, toList } from "../fable_modules/fable-library-js.5.13.0/Seq.js";

export class SemanticZoom extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Overview", "Standard", "Detailed"];
    }
    static Overview = new SemanticZoom(0, []);
    static Standard = new SemanticZoom(1, []);
    static Detailed = new SemanticZoom(2, []);
}

export function SemanticZoom_$reflection() {
    return union_type("SIR.Client.SemanticZoom", [], SemanticZoom, () => [[], [], []]);
}

export class BattlefieldCamera extends Record {
    constructor(PanX, PanY, Zoom) {
        super();
        this.PanX = PanX;
        this.PanY = PanY;
        this.Zoom = Zoom;
    }
}

export function BattlefieldCamera_$reflection() {
    return record_type("SIR.Client.BattlefieldCamera", [], BattlefieldCamera, () => [["PanX", float64_type], ["PanY", float64_type], ["Zoom", float64_type]]);
}

export class BattlefieldViewState extends Record {
    constructor(Camera, SemanticZoom, SelectedUnit, FocusedUnit, PaletteId, ExactTicks, ReducedMotion) {
        super();
        this.Camera = Camera;
        this.SemanticZoom = SemanticZoom;
        this.SelectedUnit = SelectedUnit;
        this.FocusedUnit = FocusedUnit;
        this.PaletteId = PaletteId;
        this.ExactTicks = ExactTicks;
        this.ReducedMotion = ReducedMotion;
    }
}

export function BattlefieldViewState_$reflection() {
    return record_type("SIR.Client.BattlefieldViewState", [], BattlefieldViewState, () => [["Camera", BattlefieldCamera_$reflection()], ["SemanticZoom", SemanticZoom_$reflection()], ["SelectedUnit", option_type(int32_type)], ["FocusedUnit", option_type(int32_type)], ["PaletteId", string_type], ["ExactTicks", bool_type], ["ReducedMotion", bool_type]]);
}

export class BattlefieldAction extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["PanBy", "ZoomBy", "SelectUnit", "FocusUnit", "FocusDirection", "ChoosePalette", "ChooseExactTicks", "ChooseReducedMotion", "ResetCamera"];
    }
    static ResetCamera = new BattlefieldAction(8, []);
}

export function BattlefieldAction_$reflection() {
    return union_type("SIR.Client.BattlefieldAction", [], BattlefieldAction, () => [[["x", float64_type], ["y", float64_type]], [["factor", float64_type]], [["Item", option_type(int32_type)]], [["Item", option_type(int32_type)]], [["x", int32_type], ["y", int32_type]], [["Item", string_type]], [["Item", bool_type]], [["Item", bool_type]], []]);
}

export class OverlayDisposition extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ExactOverlay", "SimplifiedSelectedOverlay", "AggregatedWholeForceOverlay", "DeclinedUnsafeOverlay"];
    }
    static ExactOverlay = new OverlayDisposition(0, []);
}

export function OverlayDisposition_$reflection() {
    return union_type("SIR.Client.OverlayDisposition", [], OverlayDisposition, () => [[], [["originalSegments", int32_type]], [["originalSegments", int32_type]], [["reason", string_type]]]);
}

export class ProjectedOverlay extends Record {
    constructor(Overlay, Points, PathSegments, Disposition) {
        super();
        this.Overlay = Overlay;
        this.Points = Points;
        this.PathSegments = (PathSegments | 0);
        this.Disposition = Disposition;
    }
}

export function ProjectedOverlay_$reflection() {
    return record_type("SIR.Client.ProjectedOverlay", [], ProjectedOverlay, () => [["Overlay", OverlayVisual_$reflection()], ["Points", array_type(float64_type)], ["PathSegments", int32_type], ["Disposition", OverlayDisposition_$reflection()]]);
}

export class ProjectedActionTrace extends Record {
    constructor(EventId, Kind, SourceX, SourceY, TargetX, TargetY) {
        super();
        this.EventId = (EventId | 0);
        this.Kind = Kind;
        this.SourceX = SourceX;
        this.SourceY = SourceY;
        this.TargetX = TargetX;
        this.TargetY = TargetY;
    }
}

export function ProjectedActionTrace_$reflection() {
    return record_type("SIR.Client.ProjectedActionTrace", [], ProjectedActionTrace, () => [["EventId", int32_type], ["Kind", string_type], ["SourceX", float64_type], ["SourceY", float64_type], ["TargetX", float64_type], ["TargetY", float64_type]]);
}

export class TimelineLane extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["AuthoritativeEvents", "UnitActions", "Communications"];
    }
    static AuthoritativeEvents = new TimelineLane(0, []);
    static UnitActions = new TimelineLane(1, []);
    static Communications = new TimelineLane(2, []);
}

export function TimelineLane_$reflection() {
    return union_type("SIR.Client.TimelineLane", [], TimelineLane, () => [[], [], []]);
}

export class TimelineItem extends Record {
    constructor(EventId, Lane, Tick, Summary) {
        super();
        this.EventId = (EventId | 0);
        this.Lane = Lane;
        this.Tick = (Tick | 0);
        this.Summary = Summary;
    }
}

export function TimelineItem_$reflection() {
    return record_type("SIR.Client.TimelineItem", [], TimelineItem, () => [["EventId", int32_type], ["Lane", TimelineLane_$reflection()], ["Tick", int32_type], ["Summary", Disclosure$1_$reflection(string_type)]]);
}

export class ProjectedUnit extends Record {
    constructor(Unit, FootprintX, FootprintY, FootprintWidth, FootprintDepth, SymbolCenterX, SymbolCenterY, HealthSegments, ElevationBars, ElevationLabel, ShowStance, AccessibleLabel) {
        super();
        this.Unit = Unit;
        this.FootprintX = FootprintX;
        this.FootprintY = FootprintY;
        this.FootprintWidth = FootprintWidth;
        this.FootprintDepth = FootprintDepth;
        this.SymbolCenterX = SymbolCenterX;
        this.SymbolCenterY = SymbolCenterY;
        this.HealthSegments = HealthSegments;
        this.ElevationBars = (ElevationBars | 0);
        this.ElevationLabel = ElevationLabel;
        this.ShowStance = ShowStance;
        this.AccessibleLabel = AccessibleLabel;
    }
}

export function ProjectedUnit_$reflection() {
    return record_type("SIR.Client.ProjectedUnit", [], ProjectedUnit, () => [["Unit", UnitVisual_$reflection()], ["FootprintX", float64_type], ["FootprintY", float64_type], ["FootprintWidth", float64_type], ["FootprintDepth", float64_type], ["SymbolCenterX", float64_type], ["SymbolCenterY", float64_type], ["HealthSegments", option_type(int32_type)], ["ElevationBars", int32_type], ["ElevationLabel", option_type(string_type)], ["ShowStance", bool_type], ["AccessibleLabel", string_type]]);
}

export class BattlefieldScene extends Record {
    constructor(Tick, Width, Height, CellSize, Board, Units, Edges, Overlays, ActionTraces, Timeline, WholeForceOverlaySegments, Disclosure, Palette, Camera, SemanticZoom, SelectedUnit, FocusedUnit, InteractiveNodeEstimate) {
        super();
        this.Tick = (Tick | 0);
        this.Width = Width;
        this.Height = Height;
        this.CellSize = CellSize;
        this.Board = Board;
        this.Units = Units;
        this.Edges = Edges;
        this.Overlays = Overlays;
        this.ActionTraces = ActionTraces;
        this.Timeline = Timeline;
        this.WholeForceOverlaySegments = (WholeForceOverlaySegments | 0);
        this.Disclosure = Disclosure;
        this.Palette = Palette;
        this.Camera = Camera;
        this.SemanticZoom = SemanticZoom;
        this.SelectedUnit = SelectedUnit;
        this.FocusedUnit = FocusedUnit;
        this.InteractiveNodeEstimate = (InteractiveNodeEstimate | 0);
    }
}

export function BattlefieldScene_$reflection() {
    return record_type("SIR.Client.BattlefieldScene", [], BattlefieldScene, () => [["Tick", int32_type], ["Width", float64_type], ["Height", float64_type], ["CellSize", float64_type], ["Board", BoardVisual_$reflection()], ["Units", array_type(ProjectedUnit_$reflection())], ["Edges", array_type(EdgeVisual_$reflection())], ["Overlays", array_type(ProjectedOverlay_$reflection())], ["ActionTraces", array_type(ProjectedActionTrace_$reflection())], ["Timeline", array_type(TimelineItem_$reflection())], ["WholeForceOverlaySegments", int32_type], ["Disclosure", DisclosureLabel_$reflection()], ["Palette", PaletteTokens_$reflection()], ["Camera", BattlefieldCamera_$reflection()], ["SemanticZoom", SemanticZoom_$reflection()], ["SelectedUnit", option_type(int32_type)], ["FocusedUnit", option_type(int32_type)], ["InteractiveNodeEstimate", int32_type]]);
}

export const Battlefield_initial = new BattlefieldViewState(new BattlefieldCamera(24, 24, 1), SemanticZoom.Detailed, 1, 1, ReplayPalettes_accessibleDefault.Id, false, false);

function Battlefield_clamp(minimum, maximum, value) {
    return max((x_1, y_1) => (compare(x_1, y_1) | 0), minimum, min((x, y) => (compare(x, y) | 0), maximum, value));
}

/**
 * Applies the 24/48 px thresholds with a ten-percent dead band.
 */
export function Battlefield_semanticZoom(previous, cellPixels) {
    const lowerEnter = 24 * (1 + 0.1);
    const lowerLeave = 24 * (1 - 0.1);
    const upperEnter = 48 * (1 + 0.1);
    const upperLeave = 48 * (1 - 0.1);
    switch (previous.tag) {
        case 1:
            if (cellPixels < lowerLeave) {
                return SemanticZoom.Overview;
            }
            else if (cellPixels >= upperEnter) {
                return SemanticZoom.Detailed;
            }
            else {
                return SemanticZoom.Standard;
            }
        case 2:
            if (cellPixels < lowerLeave) {
                return SemanticZoom.Overview;
            }
            else if (cellPixels < upperLeave) {
                return SemanticZoom.Standard;
            }
            else {
                return SemanticZoom.Detailed;
            }
        default:
            if (cellPixels >= upperEnter) {
                return SemanticZoom.Detailed;
            }
            else if (cellPixels >= lowerEnter) {
                return SemanticZoom.Standard;
            }
            else {
                return SemanticZoom.Overview;
            }
    }
}

function Battlefield_palette(paletteId) {
    return defaultArg(tryFind((candidate) => (candidate.Id === paletteId), ReplayPalettes_all), ReplayPalettes_accessibleDefault);
}

function Battlefield_disclosedOr(fallback, disclosure) {
    switch (disclosure.tag) {
        case 0:
        case 1:
        case 2:
            return fallback;
        default:
            return disclosure.fields[0];
    }
}

function Battlefield_healthSegments(disclosure) {
    switch (disclosure.tag) {
        case 0:
        case 1:
        case 2:
            return undefined;
        default: {
            const health = disclosure.fields[0];
            const remaining = toInt64_unchecked(fromInt32(HealthVisualModule_remaining(health)));
            const maximum = toInt64_unchecked(fromInt32(HealthVisualModule_maximum(health)));
            return Battlefield_clamp(0, 12, ~~toInt32_unchecked(toInt64_unchecked(op_Division(toInt64_unchecked(op_Subtraction(toInt64_unchecked(op_Addition(toInt64_unchecked(op_Multiply(remaining, 12n)), maximum)), 1n)), maximum))));
        }
    }
}

function Battlefield_compass(heading) {
    const directions = ["east", "south-east", "south", "south-west", "west", "north-west", "north", "north-east"];
    return item(~~round(HeadingRadiansModule_value(heading) / (3.141592653589793 / 4)) % 8, directions);
}

function Battlefield_cellName(column, row) {
    return (((column >= 0) && (column < 26)) ? String.fromCharCode((~~"A".charCodeAt(0) + column) & 0xFFFF) : (("column " + int32ToString(column)) + " ")) + int32ToString(row + 1);
}

function Battlefield_unitLabel(unit) {
    const glyph = UnitGlyphCatalog_resolve(unit.ClassId);
    let faction;
    const matchValue = unit.Faction;
    faction = ((matchValue.tag === 1) ? "Arcane" : ((matchValue.tag === 2) ? "Neutral" : ((matchValue.tag === 3) ? matchValue.fields[0] : "Blue")));
    let identity;
    const matchValue_1 = unit.ShortLabel;
    identity = ((matchValue_1.tag === 3) ? (" " + matchValue_1.fields[0]) : (" " + int32ToString(unit.Id)));
    let level;
    const matchValue_2 = unit.Level;
    level = ((matchValue_2.tag === 0) ? ", elevation not present in this projection" : ((matchValue_2.tag === 1) ? ", elevation not applicable" : ((matchValue_2.tag === 2) ? ", elevation explicitly unknown" : (", elevation " + int32ToString(matchValue_2.fields[0])))));
    let health;
    const matchValue_3 = unit.Health;
    switch (matchValue_3.tag) {
        case 0: {
            health = ", health not present in this projection";
            break;
        }
        case 1: {
            health = ", health not applicable";
            break;
        }
        case 2: {
            health = ", health explicitly unknown";
            break;
        }
        default: {
            const value_2 = matchValue_3.fields[0];
            health = ((", " + int32ToString(~~round((HealthVisualModule_remaining(value_2) * 100) / HealthVisualModule_maximum(value_2)))) + " health");
        }
    }
    let facing;
    const matchValue_4 = unit.BodyHeading;
    facing = ((matchValue_4.tag === 0) ? ", facing not present in this projection" : ((matchValue_4.tag === 1) ? ", facing not applicable" : ((matchValue_4.tag === 2) ? ", facing explicitly unknown" : (", facing " + Battlefield_compass(matchValue_4.fields[0])))));
    return (((((((faction + " ") + glyph.Name.toLowerCase()) + identity) + ", cell ") + Battlefield_cellName(unit.AnchorColumn, unit.AnchorRow)) + level) + health) + facing;
}

function Battlefield_projectUnit(board, tier, unit) {
    const width = CellExtentModule_value(unit.FootprintWidth) * 24;
    const depth = CellExtentModule_value(unit.FootprintDepth) * 24;
    const x = (unit.AnchorColumn - board.MinimumColumn) * 24;
    const y = (unit.AnchorRow - board.MinimumRow) * 24;
    const level = max_1(0, Battlefield_disclosedOr(0, unit.Level)) | 0;
    return new ProjectedUnit(unit, x, y, width, depth, x + (width / 2), y + (depth / 2), Battlefield_healthSegments(unit.Health), min_1(3, level), (equals(tier, SemanticZoom.Detailed) && (level > 3)) ? ("+" + int32ToString(level)) : undefined, equals(tier, SemanticZoom.Detailed) && (unit.StanceId.tag === 3), Battlefield_unitLabel(unit));
}

function Battlefield_overlaySegments(points) {
    if ((points.length < 4) ? true : ((points.length % 2) !== 0)) {
        return 0;
    }
    else {
        return (~~(points.length / 2) - 1) | 0;
    }
}

function Battlefield_validOverlayGeometry(points) {
    if (((points.length >= 4) && ((points.length % 2) === 0)) && (points.length <= 100000)) {
        return points.every((value) => {
            if (!(Number.isNaN(value) ? true : isInfinity(value))) {
                return Math.abs(value) <= 1000000;
            }
            else {
                return false;
            }
        });
    }
    else {
        return false;
    }
}

function Battlefield_simplifyTo(maximumSegments, points) {
    const vertices = ~~(points.length / 2) | 0;
    const wantedVertices = (maximumSegments + 1) | 0;
    if (vertices <= wantedVertices) {
        return copy(points);
    }
    else {
        return initialize(wantedVertices * 2, (coordinate) => {
            const targetVertex = ~~(coordinate / 2) | 0;
            return item((((targetVertex === (wantedVertices - 1)) ? (vertices - 1) : ~~((targetVertex * (vertices - 1)) / (wantedVertices - 1))) * 2) + (coordinate % 2), points);
        }, Float64Array);
    }
}

function Battlefield_boundingBox(points) {
    const xs = initialize(~~(points.length / 2), (index) => item(index * 2, points), Float64Array);
    const ys = initialize(~~(points.length / 2), (index_1) => item((index_1 * 2) + 1, points), Float64Array);
    const minimumX = min_2(xs, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const maximumX = max_2(xs, {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    });
    const minimumY = min_2(ys, {
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    });
    const maximumY = max_2(ys, {
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    });
    return new Float64Array([minimumX, minimumY, maximumX, minimumY, maximumX, maximumY, minimumX, maximumY, minimumX, minimumY]);
}

function Battlefield_boundingBoxMany(pointSets) {
    let minimumX = Infinity;
    let minimumY = Infinity;
    let maximumX = -Infinity;
    let maximumY = -Infinity;
    for (let idx = 0; idx <= (pointSets.length - 1); idx++) {
        const points = item(idx, pointSets);
        for (let index = 0; index <= (~~(points.length / 2) - 1); index++) {
            minimumX = min_1(minimumX, item(index * 2, points));
            minimumY = min_1(minimumY, item((index * 2) + 1, points));
            maximumX = max_1(maximumX, item(index * 2, points));
            maximumY = max_1(maximumY, item((index * 2) + 1, points));
        }
    }
    return Battlefield_boundingBox(new Float64Array([minimumX, minimumY, maximumX, maximumY]));
}

function Battlefield_projectOverlays(selectedUnit, overlays) {
    const patternInput = partition((overlay_1) => Battlefield_validOverlayGeometry(overlay_1.Points), overlays.filter((overlay) => {
        const matchValue = overlay.Scope;
        if (matchValue.tag === 1) {
            return true;
        }
        else {
            return equals(selectedUnit, matchValue.fields[0]);
        }
    }));
    const valid = patternInput[0];
    const wholeForceSegments = ~~toInt32_unchecked(fold((total, overlay_2) => {
        const additional = (overlay_2.Scope.tag === 0) ? (0n) : toInt64_unchecked(fromInt32(Battlefield_overlaySegments(overlay_2.Points)));
        return min_3(toInt64_unchecked(fromInt32(2147483647)), toInt64_unchecked(op_Addition(total, additional)));
    }, 0n, valid)) | 0;
    const aggregateWholeForcePoints = (wholeForceSegments > 8000) ? Battlefield_boundingBoxMany(choose((overlay_3) => {
        if (overlay_3.Scope.tag === 0) {
            return undefined;
        }
        else {
            return overlay_3.Points;
        }
    }, valid)) : undefined;
    let emittedWholeForceAggregate = false;
    return [append(map((overlay_4) => {
        const segments = Battlefield_overlaySegments(overlay_4.Points) | 0;
        if (overlay_4.Scope.tag === 1) {
            if (wholeForceSegments > 8000) {
                if (emittedWholeForceAggregate) {
                    return new ProjectedOverlay(overlay_4, new Float64Array([]), 0, new OverlayDisposition(/* DeclinedUnsafeOverlay */ 3, ["geometry represented by the combined whole-force aggregate"]));
                }
                else {
                    emittedWholeForceAggregate = true;
                    const points_1 = value_4(aggregateWholeForcePoints);
                    return new ProjectedOverlay(overlay_4, points_1, Battlefield_overlaySegments(points_1), new OverlayDisposition(/* AggregatedWholeForceOverlay */ 2, [wholeForceSegments]));
                }
            }
            else {
                return new ProjectedOverlay(overlay_4, copy(overlay_4.Points), segments, OverlayDisposition.ExactOverlay);
            }
        }
        else if (segments >= 2000) {
            const points = Battlefield_simplifyTo(2000, overlay_4.Points);
            return new ProjectedOverlay(overlay_4, points, Battlefield_overlaySegments(points), new OverlayDisposition(/* SimplifiedSelectedOverlay */ 1, [segments]));
        }
        else {
            return new ProjectedOverlay(overlay_4, copy(overlay_4.Points), segments, OverlayDisposition.ExactOverlay);
        }
    }, valid), map((overlay_5) => (new ProjectedOverlay(overlay_5, new Float64Array([]), 0, new OverlayDisposition(/* DeclinedUnsafeOverlay */ 3, ["geometry must contain bounded finite coordinate pairs"]))), patternInput[1])), wholeForceSegments];
}

function Battlefield_actionTraces(units, events) {
    const centers = ofArray(map((unit) => [unit.Unit.Id, [unit.SymbolCenterX, unit.SymbolCenterY]], units), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    return choose((event) => {
        const matchValue = event.SourceUnitId;
        const matchValue_1 = event.TargetUnitId;
        let matchResult, source, target;
        if (matchValue.tag === 3) {
            if (matchValue_1.tag === 3) {
                matchResult = 0;
                source = matchValue.fields[0];
                target = matchValue_1.fields[0];
            }
            else {
                matchResult = 1;
            }
        }
        else {
            matchResult = 1;
        }
        switch (matchResult) {
            case 0: {
                const matchValue_3 = tryFind_1(source, centers);
                const matchValue_4 = tryFind_1(target, centers);
                let matchResult_1, sourceX, sourceY, targetX, targetY;
                if (matchValue_3 != null) {
                    if (matchValue_4 != null) {
                        matchResult_1 = 0;
                        sourceX = matchValue_3[0];
                        sourceY = matchValue_3[1];
                        targetX = matchValue_4[0];
                        targetY = matchValue_4[1];
                    }
                    else {
                        matchResult_1 = 1;
                    }
                }
                else {
                    matchResult_1 = 1;
                }
                switch (matchResult_1) {
                    case 0:
                        return new ProjectedActionTrace(event.Id, event.Kind, sourceX, sourceY, targetX, targetY);
                    default:
                        return undefined;
                }
            }
            default:
                return undefined;
        }
    }, events);
}

function Battlefield_timeline(events) {
    return map((event) => {
        let matchValue;
        return new TimelineItem(event.Id, (matchValue = event.Kind, (matchValue === "communication") ? TimelineLane.Communications : ((matchValue === "acknowledgement") ? TimelineLane.Communications : ((matchValue === "move") ? TimelineLane.UnitActions : ((matchValue === "attack") ? TimelineLane.UnitActions : ((matchValue === "heal") ? TimelineLane.UnitActions : TimelineLane.AuthoritativeEvents))))), event.Tick, event.Summary);
    }, events);
}

function Battlefield_nodeEstimate(tier, units, edgeCount) {
    return ((16 + edgeCount) + sumBy((unit) => (((((18 + UnitGlyphCatalog_resolve(unit.Unit.ClassId).Primitives.length) + (equals(tier, SemanticZoom.Overview) ? 0 : (((unit.HealthSegments != null) ? 12 : 0) + unit.ElevationBars))) + ((unit.ElevationLabel != null) ? 1 : 0)) + ((equals(tier, SemanticZoom.Detailed) && unit.ShowStance) ? 1 : 0)) | 0), units, {
        GetZero: () => 0,
        Add: (x, y) => ((x + y) | 0),
    })) | 0;
}

export function Battlefield_scene(frame, state) {
    const tier = Battlefield_semanticZoom(state.SemanticZoom, 48 * state.Camera.Zoom);
    const units = map((unit) => Battlefield_projectUnit(frame.Board, tier, unit), frame.Units);
    const patternInput = Battlefield_projectOverlays(state.SelectedUnit, frame.Overlays);
    const overlays = patternInput[0];
    return new BattlefieldScene(frame.Tick, ((frame.Board.MaximumColumn - frame.Board.MinimumColumn) + 1) * 24, ((frame.Board.MaximumRow - frame.Board.MinimumRow) + 1) * 24, 24, frame.Board, units, frame.Edges, overlays, Battlefield_actionTraces(units, frame.Events), Battlefield_timeline(frame.Events), patternInput[1], frame.Disclosure, Battlefield_palette(state.PaletteId), state.Camera, tier, state.SelectedUnit, state.FocusedUnit, (Battlefield_nodeEstimate(tier, units, frame.Edges.length) + sumBy((overlay) => (max_1(1, overlay.PathSegments) | 0), overlays, {
        GetZero: () => 0,
        Add: (x, y) => ((x + y) | 0),
    })) + frame.Events.length);
}

/**
 * Applies simulator-only fractional cell offsets after authoritative frame
 * projection. These values never enter a RenderFrame, replay, collision
 * check, or authored map revision.
 */
export function Battlefield_withUnitOffsets(offsets, scene) {
    if (isEmpty(offsets)) {
        return scene;
    }
    else {
        return new BattlefieldScene(scene.Tick, scene.Width, scene.Height, scene.CellSize, scene.Board, map((unit) => {
            const matchValue = tryFind_1(unit.Unit.Id, offsets);
            if (matchValue != null) {
                const x = matchValue[0] * 24;
                const y = matchValue[1] * 24;
                return new ProjectedUnit(unit.Unit, unit.FootprintX + x, unit.FootprintY + y, unit.FootprintWidth, unit.FootprintDepth, unit.SymbolCenterX + x, unit.SymbolCenterY + y, unit.HealthSegments, unit.ElevationBars, unit.ElevationLabel, unit.ShowStance, unit.AccessibleLabel);
            }
            else {
                return unit;
            }
        }, scene.Units), scene.Edges, scene.Overlays, scene.ActionTraces, scene.Timeline, scene.WholeForceOverlaySegments, scene.Disclosure, scene.Palette, scene.Camera, scene.SemanticZoom, scene.SelectedUnit, scene.FocusedUnit, scene.InteractiveNodeEstimate);
    }
}

/**
 * Deterministic presentation-only translation. Non-position facts stay
 * on the earlier committed frame until alpha one. Spawn, disappearance,
 * footprint change, level change, or a move longer than one adjacent cell
 * is a discontinuity and is never interpolated.
 */
export function Battlefield_interpolatedScene(alpha, previous, next, state) {
    const alpha_1 = Battlefield_clamp(0, 1, alpha);
    const previousUnitIds = ofArray_1(map((_arg) => (_arg.Id | 0), previous.Units, Int32Array), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const nextUnitIds = ofArray_1(map((_arg_1) => (_arg_1.Id | 0), next.Units, Int32Array), {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    });
    const sameUnitSet = previousUnitIds.Equals(nextUnitIds);
    if (((alpha_1 >= 1) ? true : (previous.Tick === next.Tick)) ? true : !sameUnitSet) {
        return Battlefield_scene(next, state);
    }
    else {
        const earlier = Battlefield_scene(previous, state);
        const laterById = ofArray(map((unit) => [unit.Unit.Id, unit], Battlefield_scene(next, state).Units), {
            Compare: (x_4, y_4) => (comparePrimitives(x_4, y_4) | 0),
        });
        const nextRaw = ofArray(map((unit_1) => [unit_1.Id, unit_1], next.Units), {
            Compare: (x_5, y_5) => (comparePrimitives(x_5, y_5) | 0),
        });
        const units = map((unit_2) => {
            let targetRaw, fromUnit, toUnit, overlaps, array_4;
            const matchValue_3 = tryFind_1(unit_2.Unit.Id, laterById);
            const matchValue_4 = tryFind_1(unit_2.Unit.Id, nextRaw);
            let matchResult, target_1, targetRaw_1;
            if (matchValue_3 != null) {
                if (matchValue_4 != null) {
                    if ((targetRaw = matchValue_4, (((equals(unit_2.Unit.Level, targetRaw.Level) && equals(unit_2.Unit.FootprintWidth, targetRaw.FootprintWidth)) && equals(unit_2.Unit.FootprintDepth, targetRaw.FootprintDepth)) && (max_1(Math.abs(targetRaw.AnchorColumn - unit_2.Unit.AnchorColumn), Math.abs(targetRaw.AnchorRow - unit_2.Unit.AnchorRow)) <= 1)) && !((fromUnit = unit_2.Unit, (toUnit = targetRaw, (overlaps = ((firstStart, firstEnd, secondStart, secondEnd) => (compare(max((x_2, y_2) => (compare(x_2, y_2) | 0), firstStart, secondStart), min((x_3, y_3) => (compare(x_3, y_3) | 0), firstEnd, secondEnd)) < 0)), (array_4 = append(previous.Edges, next.Edges), array_4.some((edge_2) => {
                        let edge, matchValue, matchValue_1;
                        if ((edge = edge_2, (matchValue = edge.Kind, (matchValue_1 = edge.State, (matchValue === "door") ? (!(matchValue_1 === "open")) : ((matchValue === "wall") ? true : ((matchValue === "fence") && (matchValue_1 === "closed"))))))) {
                            const edge_1 = edge_2;
                            const fromColumn = fromUnit.AnchorColumn | 0;
                            const fromRow = fromUnit.AnchorRow | 0;
                            const columnDelta = (toUnit.AnchorColumn - fromColumn) | 0;
                            const rowDelta = (toUnit.AnchorRow - fromRow) | 0;
                            const width = CellExtentModule_value(fromUnit.FootprintWidth) | 0;
                            const depth = CellExtentModule_value(fromUnit.FootprintDepth) | 0;
                            let crossesVertical;
                            if (columnDelta === 0) {
                                crossesVertical = false;
                            }
                            else {
                                const boundaryColumn = ((columnDelta > 0) ? (fromColumn + width) : fromColumn) | 0;
                                crossesVertical = (((edge_1.StartColumn === boundaryColumn) && (edge_1.EndColumn === boundaryColumn)) && overlaps(min_1(edge_1.StartRow, edge_1.EndRow), max_1(edge_1.StartRow, edge_1.EndRow), fromRow, fromRow + depth));
                            }
                            let crossesHorizontal;
                            if (rowDelta === 0) {
                                crossesHorizontal = false;
                            }
                            else {
                                const boundaryRow = ((rowDelta > 0) ? (fromRow + depth) : fromRow) | 0;
                                crossesHorizontal = (((edge_1.StartRow === boundaryRow) && (edge_1.EndRow === boundaryRow)) && overlaps(min_1(edge_1.StartColumn, edge_1.EndColumn), max_1(edge_1.StartColumn, edge_1.EndColumn), fromColumn, fromColumn + width));
                            }
                            if (crossesVertical) {
                                return true;
                            }
                            else {
                                return crossesHorizontal;
                            }
                        }
                        else {
                            return false;
                        }
                    })))))))) {
                        matchResult = 0;
                        target_1 = matchValue_3;
                        targetRaw_1 = matchValue_4;
                    }
                    else {
                        matchResult = 1;
                    }
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
            switch (matchResult) {
                case 0: {
                    const lerp = (start, finish) => (start + ((finish - start) * alpha_1));
                    const footprintX = lerp(unit_2.FootprintX, target_1.FootprintX);
                    const footprintY = lerp(unit_2.FootprintY, target_1.FootprintY);
                    return new ProjectedUnit(unit_2.Unit, footprintX, footprintY, unit_2.FootprintWidth, unit_2.FootprintDepth, footprintX + (unit_2.FootprintWidth / 2), footprintY + (unit_2.FootprintDepth / 2), unit_2.HealthSegments, unit_2.ElevationBars, unit_2.ElevationLabel, unit_2.ShowStance, unit_2.AccessibleLabel);
                }
                default:
                    return unit_2;
            }
        }, earlier.Units);
        return new BattlefieldScene(earlier.Tick, earlier.Width, earlier.Height, earlier.CellSize, earlier.Board, units, earlier.Edges, earlier.Overlays, Battlefield_actionTraces(units, previous.Events), earlier.Timeline, earlier.WholeForceOverlaySegments, earlier.Disclosure, earlier.Palette, earlier.Camera, earlier.SemanticZoom, earlier.SelectedUnit, earlier.FocusedUnit, earlier.InteractiveNodeEstimate);
    }
}

function Battlefield_directionalFocus(xDirection, yDirection, units, focused) {
    let option_5;
    let current;
    let option_3;
    const option_1 = focused;
    if (option_1 != null) {
        const id = option_1 | 0;
        option_3 = tryFind((unit) => (unit.Id === id), units);
    }
    else {
        option_3 = undefined;
    }
    current = ((option_3 != null) ? option_3 : tryHead(units));
    if (current != null) {
        const current_1 = current;
        return orElse((option_5 = tryHead(sortBy((candidate_1) => {
            const dx = (candidate_1.AnchorColumn - current_1.AnchorColumn) | 0;
            const dy = (candidate_1.AnchorRow - current_1.AnchorRow) | 0;
            return [(dx * dx) + (dy * dy), candidate_1.Id];
        }, units.filter((candidate) => {
            if ((candidate.Id !== current_1.Id) && ((xDirection === 0) ? true : (sign(candidate.AnchorColumn - current_1.AnchorColumn) === xDirection))) {
                if (yDirection === 0) {
                    return true;
                }
                else {
                    return sign(candidate.AnchorRow - current_1.AnchorRow) === yDirection;
                }
            }
            else {
                return false;
            }
        }), {
            Compare: (x, y) => (compareArrays(x, y) | 0),
        })), (option_5 != null) ? option_5.Id : undefined), current_1.Id);
    }
    else {
        return undefined;
    }
}

export function Battlefield_update(frame, action, state) {
    let bind$0040_1;
    switch (action.tag) {
        case 1: {
            const zoom = Battlefield_clamp(0.35, 3, state.Camera.Zoom * action.fields[0]);
            return new BattlefieldViewState((bind$0040_1 = state.Camera, new BattlefieldCamera(bind$0040_1.PanX, bind$0040_1.PanY, zoom)), Battlefield_semanticZoom(state.SemanticZoom, 48 * zoom), state.SelectedUnit, state.FocusedUnit, state.PaletteId, state.ExactTicks, state.ReducedMotion);
        }
        case 2:
            return new BattlefieldViewState(state.Camera, state.SemanticZoom, action.fields[0], state.FocusedUnit, state.PaletteId, state.ExactTicks, state.ReducedMotion);
        case 3:
            return new BattlefieldViewState(state.Camera, state.SemanticZoom, state.SelectedUnit, action.fields[0], state.PaletteId, state.ExactTicks, state.ReducedMotion);
        case 4:
            return new BattlefieldViewState(state.Camera, state.SemanticZoom, state.SelectedUnit, Battlefield_directionalFocus(action.fields[0], action.fields[1], frame.Units, state.FocusedUnit), state.PaletteId, state.ExactTicks, state.ReducedMotion);
        case 5: {
            const paletteId = action.fields[0];
            if (ReplayPalettes_all.some((p) => (p.Id === paletteId))) {
                return new BattlefieldViewState(state.Camera, state.SemanticZoom, state.SelectedUnit, state.FocusedUnit, paletteId, state.ExactTicks, state.ReducedMotion);
            }
            else {
                return state;
            }
        }
        case 6:
            return new BattlefieldViewState(state.Camera, state.SemanticZoom, state.SelectedUnit, state.FocusedUnit, state.PaletteId, action.fields[0], state.ReducedMotion);
        case 7:
            return new BattlefieldViewState(state.Camera, state.SemanticZoom, state.SelectedUnit, state.FocusedUnit, state.PaletteId, state.ExactTicks, action.fields[0]);
        case 8:
            return new BattlefieldViewState(Battlefield_initial.Camera, Battlefield_initial.SemanticZoom, Battlefield_initial.SelectedUnit, Battlefield_initial.FocusedUnit, state.PaletteId, state.ExactTicks, state.ReducedMotion);
        default:
            return new BattlefieldViewState(new BattlefieldCamera(state.Camera.PanX + action.fields[0], state.Camera.PanY + action.fields[1], state.Camera.Zoom), state.SemanticZoom, state.SelectedUnit, state.FocusedUnit, state.PaletteId, state.ExactTicks, state.ReducedMotion);
    }
}

/**
 * Removes interaction state for entities that are no longer disclosed.
 */
export function Battlefield_reconcile(frame, state) {
    const disclosed = ofArray_1(map((_arg) => (_arg.Id | 0), frame.Units, Int32Array), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const keep = (id) => {
        const option_1 = id;
        if (option_1 != null) {
            if (contains(option_1, disclosed)) {
                return option_1;
            }
            else {
                return undefined;
            }
        }
        else {
            return undefined;
        }
    };
    return new BattlefieldViewState(state.Camera, state.SemanticZoom, keep(state.SelectedUnit), keep(state.FocusedUnit), state.PaletteId, state.ExactTicks, state.ReducedMotion);
}

function Battlefield_extent(value) {
    const option_1 = CellExtentModule_tryCreate(value);
    if (option_1 != null) {
        return option_1;
    }
    else {
        throw new Exception("Extent must be positive. (Parameter \'value\')");
    }
}

function Battlefield_heading(value) {
    const option_1 = HeadingRadiansModule_tryCreate(value);
    if (option_1 != null) {
        return option_1;
    }
    else {
        throw new Exception("Heading must be finite. (Parameter \'value\')");
    }
}

function Battlefield_health(remaining) {
    const option_1 = HealthVisualModule_tryCreate(remaining, 12);
    if (option_1 != null) {
        return option_1;
    }
    else {
        throw new Exception("Health must be 0 through 12. (Parameter \'remaining\')");
    }
}

function Battlefield_sampleUnit(id, column, row, baseSize, classId, faction, remaining, level, stance, headingRadians, label) {
    let option_1;
    return new UnitVisual(id, column, row, Battlefield_extent(baseSize), Battlefield_extent(baseSize), UnitClassIdModule_resolve(classId), faction, new Disclosure$1(/* Disclosed */ 3, [Battlefield_health(remaining)]), new Disclosure$1(/* Disclosed */ 3, [level]), defaultArg((option_1 = stance, (option_1 != null) ? (new Disclosure$1(/* Disclosed */ 3, [option_1])) : undefined), Disclosure$1.NotApplicable), new Disclosure$1(/* Disclosed */ 3, [Battlefield_heading(headingRadians)]), Disclosure$1.NotPresent, new Disclosure$1(/* Disclosed */ 3, [label]), []);
}

function Battlefield_withSecondary(source, radians, unit) {
    return new UnitVisual(unit.Id, unit.AnchorColumn, unit.AnchorRow, unit.FootprintWidth, unit.FootprintDepth, unit.ClassId, unit.Faction, unit.Health, unit.Level, unit.StanceId, unit.BodyHeading, new Disclosure$1(/* Disclosed */ 3, [new SecondaryHeadingVisual(Battlefield_heading(radians), source)]), unit.ShortLabel, unit.StatusIds);
}

export const Battlefield_representativeFrame = new RenderFrame(24, new BoardVisual(0, 0, 15, 15), [Battlefield_withSecondary(SecondaryHeadingSource.WeaponHeading, 3.141592653589793 / 4, Battlefield_sampleUnit(1, 0, 0, 4, "rifleman", FactionVisual.Human, 12, 0, "standing", 0, "Bravo 6")), Battlefield_withSecondary(SecondaryHeadingSource.SensorHeading, 3.141592653589793 * 1.25, Battlefield_sampleUnit(2, 6, 0, 4, "medic", FactionVisual.Human, 9, 2, "kneeling", 3.141592653589793 / 4, "Mercy")), Battlefield_sampleUnit(3, 0, 4, 4, "gunner", FactionVisual.Human, 6, 4, "prone", 3.141592653589793 * 1.5, "Anvil"), Battlefield_sampleUnit(4, 12, 4, 2, "observation-drone", FactionVisual.Neutral, 11, 1, undefined, 3.141592653589793, "Kite"), Battlefield_sampleUnit(5, 12, 12, 2, "goblin", FactionVisual.Arcane, 8, 0, "crouched", 3.141592653589793, "Needle"), Battlefield_sampleUnit(6, 6, 10, 4, "troll", FactionVisual.Arcane, 3, 7, "braced", 3.141592653589793 * 1.25, "Stone")], [new EdgeVisual("wall-north", "wall", "solid", 0, 3, 3, 3), new EdgeVisual("door-east", "door", "open", 3, 1, 3, 2), new EdgeVisual("window", "window", "intact", 1, 1, 2, 1)], [new OverlayVisual("selected-los-1", "line-of-sight", new OverlayScope(/* SelectedUnitOverlay */ 0, [1]), 1, new Float64Array([48, 48, 120, 72, 216, 168, 312, 312]), new Disclosure$1(/* Disclosed */ 3, ["Exact selected line of sight"])), new OverlayVisual("whole-command-1", "command", OverlayScope.WholeForceOverlay, 1, new Float64Array([12, 12, 372, 12, 372, 180, 12, 180, 12, 12]), new Disclosure$1(/* Disclosed */ 3, ["Whole-force command area"]))], [new RenderEventVisual(2401, 24, "attack", new Disclosure$1(/* Disclosed */ 3, [1]), new Disclosure$1(/* Disclosed */ 3, [5]), new Disclosure$1(/* Disclosed */ 3, ["Bravo 6 attacks Needle"])), new RenderEventVisual(2402, 24, "communication", new Disclosure$1(/* Disclosed */ 3, [2]), new Disclosure$1(/* Disclosed */ 3, [1]), new Disclosure$1(/* Disclosed */ 3, ["Mercy acknowledges Bravo 6"]))], DisclosureLabel.SandboxDisclosure);

export function Battlefield_performanceFrame(unitCount) {
    return new RenderFrame(100, new BoardVisual(0, 0, 20 - 1, max_1(1, ~~(((unitCount + 20) - 1) / 20)) - 1), initialize(unitCount, (index) => {
        const faction = ((index % 2) === 0) ? FactionVisual.Human : FactionVisual.Arcane;
        return Battlefield_sampleUnit(index + 1, index % 20, ~~(index / 20), 1, ((index % 2) === 0) ? "rifleman" : "goblin", faction, index % 13, index % 8, ((index % 3) === 0) ? "kneeling" : undefined, ((index % 8) * 3.141592653589793) / 4, "Unit " + int32ToString(index + 1));
    }), [], Battlefield_representativeFrame.Overlays, Battlefield_representativeFrame.Events, Battlefield_representativeFrame.Disclosure);
}

/**
 * Stable evidence text useful in reviews without treating pixels as authority.
 */
export function Battlefield_deterministicEvidence(scene) {
    const number = (value) => format('{0:' + "0.###" + '}', value);
    return join("\n", toList(delay(() => append_1(singleton("tick=" + int32ToString(scene.Tick)), delay(() => append_1(singleton((("board=" + number(scene.Width)) + "x") + number(scene.Height)), delay(() => append_1(singleton("tier=" + toString(scene.SemanticZoom)), delay(() => append_1(singleton("palette=" + scene.Palette.Id), delay(() => append_1(singleton((((("camera=" + number(scene.Camera.PanX)) + ",") + number(scene.Camera.PanY)) + ",") + number(scene.Camera.Zoom)), delay(() => map((unit) => {
        let option_1;
        return (((((((((((((("unit=" + int32ToString(unit.Unit.Id)) + "@") + number(unit.FootprintX)) + ",") + number(unit.FootprintY)) + ":") + number(unit.FootprintWidth)) + "x") + number(unit.FootprintDepth)) + ":health=") + defaultArg((option_1 = unit.HealthSegments, (option_1 != null) ? int32ToString(option_1) : undefined), "omitted")) + ":elevation=") + int32ToString(unit.ElevationBars)) + ":stance=") + toString(unit.ShowStance);
    }, scene.Units))))))))))))));
}

