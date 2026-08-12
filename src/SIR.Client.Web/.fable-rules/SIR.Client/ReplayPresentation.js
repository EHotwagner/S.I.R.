
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { option_type, array_type, record_type, int32_type, float64_type, string_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { contains, ofList } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { ofArray } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { int32ToString, Exception, comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { isInfinity } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { value as value_2, some } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { isNullOrWhiteSpace } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { map, copy } from "../fable_modules/fable-library-js.5.13.0/Array.js";

/**
 * The reason a presentation field has no disclosed value.
 */
export class Disclosure$1 extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["NotPresent", "NotApplicable", "ExplicitlyUnknown", "Disclosed"];
    }
    static NotPresent = new Disclosure$1(0, []);
    static NotApplicable = new Disclosure$1(1, []);
    static ExplicitlyUnknown = new Disclosure$1(2, []);
}

export function Disclosure$1_$reflection(gen0) {
    return union_type("SIR.Client.Disclosure`1", [gen0], Disclosure$1, () => [[], [], [], [["Item", gen0]]]);
}

/**
 * A stable identifier from the built-in unit glyph catalog.
 */
export class UnitClassId extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["UnitClassId"];
    }
}

export function UnitClassId_$reflection() {
    return union_type("SIR.Client.UnitClassId", [], UnitClassId, () => [[["Item", string_type]]]);
}

export function UnitClassIdModule_value(_arg) {
    return _arg.fields[0];
}

const UnitClassIdModule_known = ofList(ofArray(["rifleman", "gunner", "marksman", "engineer", "medic", "signaller", "observation-drone", "relay-drone", "goblin", "orc", "troll", "senior-caster", "magical-assistant", "ambient-critter"]), {
    Compare: (x, y) => (comparePrimitives(x, y) | 0),
});

export const UnitClassIdModule_placeholder = new UnitClassId("unknown-unit");

/**
 * Resolves untrusted class text to a built-in identifier.
 */
export function UnitClassIdModule_resolve(value) {
    if (contains(value, UnitClassIdModule_known)) {
        return new UnitClassId(value);
    }
    else {
        return UnitClassIdModule_placeholder;
    }
}

/**
 * A normalized absolute heading in radians.
 */
export class HeadingRadians extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["HeadingRadians"];
    }
}

export function HeadingRadians_$reflection() {
    return union_type("SIR.Client.HeadingRadians", [], HeadingRadians, () => [[["Item", float64_type]]]);
}

const HeadingRadiansModule_fullTurn = 3.141592653589793 * 2;

export function HeadingRadiansModule_tryCreate(value) {
    if (Number.isNaN(value) ? true : isInfinity(value)) {
        return undefined;
    }
    else {
        return new HeadingRadians(((value % HeadingRadiansModule_fullTurn) + HeadingRadiansModule_fullTurn) % HeadingRadiansModule_fullTurn);
    }
}

export function HeadingRadiansModule_value(_arg) {
    return _arg.fields[0];
}

export function HeadingRadiansModule_ofDirection8(direction) {
    const option_1 = HeadingRadiansModule_tryCreate((direction.tag === 1) ? (-3.141592653589793 / 4) : ((direction.tag === 2) ? 0 : ((direction.tag === 3) ? (3.141592653589793 / 4) : ((direction.tag === 4) ? (3.141592653589793 / 2) : ((direction.tag === 5) ? ((3.141592653589793 * 3) / 4) : ((direction.tag === 6) ? 3.141592653589793 : ((direction.tag === 7) ? ((3.141592653589793 * 5) / 4) : (-3.141592653589793 / 2))))))));
    if (option_1 != null) {
        return option_1;
    }
    else {
        throw new Exception("Canonical direction produced an invalid heading.");
    }
}

/**
 * A positive cell extent used by an authoritative footprint.
 */
export class CellExtent extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["CellExtent"];
    }
}

export function CellExtent_$reflection() {
    return union_type("SIR.Client.CellExtent", [], CellExtent, () => [[["Item", int32_type]]]);
}

export function CellExtentModule_tryCreate(value) {
    if (value > 0) {
        return new CellExtent(value);
    }
    else {
        return undefined;
    }
}

export function CellExtentModule_value(_arg) {
    return _arg.fields[0] | 0;
}

export class FactionVisual extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Human", "Arcane", "Neutral", "OtherFaction"];
    }
    static Human = new FactionVisual(0, []);
    static Arcane = new FactionVisual(1, []);
    static Neutral = new FactionVisual(2, []);
}

export function FactionVisual_$reflection() {
    return union_type("SIR.Client.FactionVisual", [], FactionVisual, () => [[], [], [], [["stableId", string_type]]]);
}

export class HealthVisual extends Record {
    constructor(Remaining, Maximum) {
        super();
        this.Remaining = (Remaining | 0);
        this.Maximum = (Maximum | 0);
    }
}

export function HealthVisual_$reflection() {
    return record_type("SIR.Client.HealthVisual", [], HealthVisual, () => [["Remaining", int32_type], ["Maximum", int32_type]]);
}

export function HealthVisualModule_tryCreate(remaining, maximum) {
    if (((maximum > 0) && (remaining >= 0)) && (remaining <= maximum)) {
        return new HealthVisual(remaining, maximum);
    }
    else {
        return undefined;
    }
}

export function HealthVisualModule_remaining(health) {
    return health.Remaining | 0;
}

export function HealthVisualModule_maximum(health) {
    return health.Maximum | 0;
}

export class UnitVisual extends Record {
    constructor(Id, AnchorColumn, AnchorRow, FootprintWidth, FootprintDepth, ClassId, Faction, Health, Level, StanceId, BodyHeading, SecondaryHeading, ShortLabel, StatusIds) {
        super();
        this.Id = (Id | 0);
        this.AnchorColumn = (AnchorColumn | 0);
        this.AnchorRow = (AnchorRow | 0);
        this.FootprintWidth = FootprintWidth;
        this.FootprintDepth = FootprintDepth;
        this.ClassId = ClassId;
        this.Faction = Faction;
        this.Health = Health;
        this.Level = Level;
        this.StanceId = StanceId;
        this.BodyHeading = BodyHeading;
        this.SecondaryHeading = SecondaryHeading;
        this.ShortLabel = ShortLabel;
        this.StatusIds = StatusIds;
    }
}

export function UnitVisual_$reflection() {
    return record_type("SIR.Client.UnitVisual", [], UnitVisual, () => [["Id", int32_type], ["AnchorColumn", int32_type], ["AnchorRow", int32_type], ["FootprintWidth", CellExtent_$reflection()], ["FootprintDepth", CellExtent_$reflection()], ["ClassId", UnitClassId_$reflection()], ["Faction", FactionVisual_$reflection()], ["Health", Disclosure$1_$reflection(HealthVisual_$reflection())], ["Level", Disclosure$1_$reflection(int32_type)], ["StanceId", Disclosure$1_$reflection(string_type)], ["BodyHeading", Disclosure$1_$reflection(HeadingRadians_$reflection())], ["SecondaryHeading", Disclosure$1_$reflection(SecondaryHeadingVisual_$reflection())], ["ShortLabel", Disclosure$1_$reflection(string_type)], ["StatusIds", array_type(string_type)]]);
}

/**
 * The accepted gameplay channel that explicitly disclosed attention or a
 * legacy capability-specific secondary heading.
 */
export class SecondaryHeadingSource extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["AttentionHeading", "WeaponHeading", "SensorHeading"];
    }
    static AttentionHeading = new SecondaryHeadingSource(0, []);
    static WeaponHeading = new SecondaryHeadingSource(1, []);
    static SensorHeading = new SecondaryHeadingSource(2, []);
}

export function SecondaryHeadingSource_$reflection() {
    return union_type("SIR.Client.SecondaryHeadingSource", [], SecondaryHeadingSource, () => [[], [], []]);
}

export class SecondaryHeadingVisual extends Record {
    constructor(Radians, Source) {
        super();
        this.Radians = Radians;
        this.Source = Source;
    }
}

export function SecondaryHeadingVisual_$reflection() {
    return record_type("SIR.Client.SecondaryHeadingVisual", [], SecondaryHeadingVisual, () => [["Radians", HeadingRadians_$reflection()], ["Source", SecondaryHeadingSource_$reflection()]]);
}

export class BoardVisual extends Record {
    constructor(MinimumColumn, MinimumRow, MaximumColumn, MaximumRow) {
        super();
        this.MinimumColumn = (MinimumColumn | 0);
        this.MinimumRow = (MinimumRow | 0);
        this.MaximumColumn = (MaximumColumn | 0);
        this.MaximumRow = (MaximumRow | 0);
    }
}

export function BoardVisual_$reflection() {
    return record_type("SIR.Client.BoardVisual", [], BoardVisual, () => [["MinimumColumn", int32_type], ["MinimumRow", int32_type], ["MaximumColumn", int32_type], ["MaximumRow", int32_type]]);
}

export class EdgeVisual extends Record {
    constructor(Id, Kind, State, StartColumn, StartRow, EndColumn, EndRow) {
        super();
        this.Id = Id;
        this.Kind = Kind;
        this.State = State;
        this.StartColumn = (StartColumn | 0);
        this.StartRow = (StartRow | 0);
        this.EndColumn = (EndColumn | 0);
        this.EndRow = (EndRow | 0);
    }
}

export function EdgeVisual_$reflection() {
    return record_type("SIR.Client.EdgeVisual", [], EdgeVisual, () => [["Id", string_type], ["Kind", string_type], ["State", string_type], ["StartColumn", int32_type], ["StartRow", int32_type], ["EndColumn", int32_type], ["EndRow", int32_type]]);
}

export class OverlayScope extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["SelectedUnitOverlay", "WholeForceOverlay"];
    }
    static WholeForceOverlay = new OverlayScope(1, []);
}

export function OverlayScope_$reflection() {
    return union_type("SIR.Client.OverlayScope", [], OverlayScope, () => [[["unitId", int32_type]], []]);
}

export class OverlayVisual extends Record {
    constructor(Id, Kind, Scope, GeometryRevision, Points, Label) {
        super();
        this.Id = Id;
        this.Kind = Kind;
        this.Scope = Scope;
        this.GeometryRevision = (GeometryRevision | 0);
        this.Points = Points;
        this.Label = Label;
    }
}

export function OverlayVisual_$reflection() {
    return record_type("SIR.Client.OverlayVisual", [], OverlayVisual, () => [["Id", string_type], ["Kind", string_type], ["Scope", OverlayScope_$reflection()], ["GeometryRevision", int32_type], ["Points", array_type(float64_type)], ["Label", Disclosure$1_$reflection(string_type)]]);
}

export class RenderEventVisual extends Record {
    constructor(Id, Tick, Kind, SourceUnitId, TargetUnitId, Summary) {
        super();
        this.Id = (Id | 0);
        this.Tick = (Tick | 0);
        this.Kind = Kind;
        this.SourceUnitId = SourceUnitId;
        this.TargetUnitId = TargetUnitId;
        this.Summary = Summary;
    }
}

export function RenderEventVisual_$reflection() {
    return record_type("SIR.Client.RenderEventVisual", [], RenderEventVisual, () => [["Id", int32_type], ["Tick", int32_type], ["Kind", string_type], ["SourceUnitId", Disclosure$1_$reflection(int32_type)], ["TargetUnitId", Disclosure$1_$reflection(int32_type)], ["Summary", Disclosure$1_$reflection(string_type)]]);
}

export class DisclosureLabel extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["FullReplayDisclosure", "PerspectiveDisclosure", "SandboxDisclosure"];
    }
    static FullReplayDisclosure = new DisclosureLabel(0, []);
    static PerspectiveDisclosure = new DisclosureLabel(1, []);
    static SandboxDisclosure = new DisclosureLabel(2, []);
}

export function DisclosureLabel_$reflection() {
    return union_type("SIR.Client.DisclosureLabel", [], DisclosureLabel, () => [[], [], []]);
}

/**
 * One independently drawable, committed presentation frame.
 */
export class RenderFrame extends Record {
    constructor(Tick, Board, Units, Edges, Overlays, Events, Disclosure) {
        super();
        this.Tick = (Tick | 0);
        this.Board = Board;
        this.Units = Units;
        this.Edges = Edges;
        this.Overlays = Overlays;
        this.Events = Events;
        this.Disclosure = Disclosure;
    }
}

export function RenderFrame_$reflection() {
    return record_type("SIR.Client.RenderFrame", [], RenderFrame, () => [["Tick", int32_type], ["Board", BoardVisual_$reflection()], ["Units", array_type(UnitVisual_$reflection())], ["Edges", array_type(EdgeVisual_$reflection())], ["Overlays", array_type(OverlayVisual_$reflection())], ["Events", array_type(RenderEventVisual_$reflection())], ["Disclosure", DisclosureLabel_$reflection()]]);
}

/**
 * Scalar/array-only wire representation for the browser structured-clone boundary.
 */
export class UnitVisualTransport extends Record {
    constructor(Id, AnchorColumn, AnchorRow, FootprintWidth, FootprintDepth, ClassId, FactionKind, FactionId, HealthKind, HealthRemaining, HealthMaximum, LevelKind, Level, StanceKind, StanceId, BodyHeadingKind, BodyHeadingRadians, SecondaryHeadingKind, SecondaryHeadingRadians, SecondaryHeadingSource, ShortLabelKind, ShortLabel, StatusIds) {
        super();
        this.Id = (Id | 0);
        this.AnchorColumn = (AnchorColumn | 0);
        this.AnchorRow = (AnchorRow | 0);
        this.FootprintWidth = (FootprintWidth | 0);
        this.FootprintDepth = (FootprintDepth | 0);
        this.ClassId = ClassId;
        this.FactionKind = (FactionKind | 0);
        this.FactionId = FactionId;
        this.HealthKind = (HealthKind | 0);
        this.HealthRemaining = HealthRemaining;
        this.HealthMaximum = HealthMaximum;
        this.LevelKind = (LevelKind | 0);
        this.Level = Level;
        this.StanceKind = (StanceKind | 0);
        this.StanceId = StanceId;
        this.BodyHeadingKind = (BodyHeadingKind | 0);
        this.BodyHeadingRadians = BodyHeadingRadians;
        this.SecondaryHeadingKind = (SecondaryHeadingKind | 0);
        this.SecondaryHeadingRadians = SecondaryHeadingRadians;
        this.SecondaryHeadingSource = SecondaryHeadingSource;
        this.ShortLabelKind = (ShortLabelKind | 0);
        this.ShortLabel = ShortLabel;
        this.StatusIds = StatusIds;
    }
}

export function UnitVisualTransport_$reflection() {
    return record_type("SIR.Client.UnitVisualTransport", [], UnitVisualTransport, () => [["Id", int32_type], ["AnchorColumn", int32_type], ["AnchorRow", int32_type], ["FootprintWidth", int32_type], ["FootprintDepth", int32_type], ["ClassId", string_type], ["FactionKind", int32_type], ["FactionId", option_type(string_type)], ["HealthKind", int32_type], ["HealthRemaining", option_type(int32_type)], ["HealthMaximum", option_type(int32_type)], ["LevelKind", int32_type], ["Level", option_type(int32_type)], ["StanceKind", int32_type], ["StanceId", option_type(string_type)], ["BodyHeadingKind", int32_type], ["BodyHeadingRadians", option_type(float64_type)], ["SecondaryHeadingKind", int32_type], ["SecondaryHeadingRadians", option_type(float64_type)], ["SecondaryHeadingSource", option_type(int32_type)], ["ShortLabelKind", int32_type], ["ShortLabel", option_type(string_type)], ["StatusIds", array_type(string_type)]]);
}

export class EdgeVisualTransport extends Record {
    constructor(Id, Kind, State, StartColumn, StartRow, EndColumn, EndRow) {
        super();
        this.Id = Id;
        this.Kind = Kind;
        this.State = State;
        this.StartColumn = (StartColumn | 0);
        this.StartRow = (StartRow | 0);
        this.EndColumn = (EndColumn | 0);
        this.EndRow = (EndRow | 0);
    }
}

export function EdgeVisualTransport_$reflection() {
    return record_type("SIR.Client.EdgeVisualTransport", [], EdgeVisualTransport, () => [["Id", string_type], ["Kind", string_type], ["State", string_type], ["StartColumn", int32_type], ["StartRow", int32_type], ["EndColumn", int32_type], ["EndRow", int32_type]]);
}

export class OverlayVisualTransport extends Record {
    constructor(Id, Kind, ScopeKind, ScopeUnitId, GeometryRevision, Points, LabelKind, Label) {
        super();
        this.Id = Id;
        this.Kind = Kind;
        this.ScopeKind = (ScopeKind | 0);
        this.ScopeUnitId = ScopeUnitId;
        this.GeometryRevision = (GeometryRevision | 0);
        this.Points = Points;
        this.LabelKind = (LabelKind | 0);
        this.Label = Label;
    }
}

export function OverlayVisualTransport_$reflection() {
    return record_type("SIR.Client.OverlayVisualTransport", [], OverlayVisualTransport, () => [["Id", string_type], ["Kind", string_type], ["ScopeKind", int32_type], ["ScopeUnitId", option_type(int32_type)], ["GeometryRevision", int32_type], ["Points", array_type(float64_type)], ["LabelKind", int32_type], ["Label", option_type(string_type)]]);
}

export class RenderEventVisualTransport extends Record {
    constructor(Id, Tick, Kind, SourceUnitIdKind, SourceUnitId, TargetUnitIdKind, TargetUnitId, SummaryKind, Summary) {
        super();
        this.Id = (Id | 0);
        this.Tick = (Tick | 0);
        this.Kind = Kind;
        this.SourceUnitIdKind = (SourceUnitIdKind | 0);
        this.SourceUnitId = SourceUnitId;
        this.TargetUnitIdKind = (TargetUnitIdKind | 0);
        this.TargetUnitId = TargetUnitId;
        this.SummaryKind = (SummaryKind | 0);
        this.Summary = Summary;
    }
}

export function RenderEventVisualTransport_$reflection() {
    return record_type("SIR.Client.RenderEventVisualTransport", [], RenderEventVisualTransport, () => [["Id", int32_type], ["Tick", int32_type], ["Kind", string_type], ["SourceUnitIdKind", int32_type], ["SourceUnitId", option_type(int32_type)], ["TargetUnitIdKind", int32_type], ["TargetUnitId", option_type(int32_type)], ["SummaryKind", int32_type], ["Summary", option_type(string_type)]]);
}

export class RenderFrameTransport extends Record {
    constructor(Tick, BoardMinimumColumn, BoardMinimumRow, BoardMaximumColumn, BoardMaximumRow, Units, Edges, Overlays, Events, Disclosure) {
        super();
        this.Tick = (Tick | 0);
        this.BoardMinimumColumn = (BoardMinimumColumn | 0);
        this.BoardMinimumRow = (BoardMinimumRow | 0);
        this.BoardMaximumColumn = (BoardMaximumColumn | 0);
        this.BoardMaximumRow = (BoardMaximumRow | 0);
        this.Units = Units;
        this.Edges = Edges;
        this.Overlays = Overlays;
        this.Events = Events;
        this.Disclosure = (Disclosure | 0);
    }
}

export function RenderFrameTransport_$reflection() {
    return record_type("SIR.Client.RenderFrameTransport", [], RenderFrameTransport, () => [["Tick", int32_type], ["BoardMinimumColumn", int32_type], ["BoardMinimumRow", int32_type], ["BoardMaximumColumn", int32_type], ["BoardMaximumRow", int32_type], ["Units", array_type(UnitVisualTransport_$reflection())], ["Edges", array_type(EdgeVisualTransport_$reflection())], ["Overlays", array_type(OverlayVisualTransport_$reflection())], ["Events", array_type(RenderEventVisualTransport_$reflection())], ["Disclosure", int32_type]]);
}

function RenderFrameTransportModule_disclosureToTransport(value) {
    switch (value.tag) {
        case 1:
            return [1, undefined];
        case 2:
            return [2, undefined];
        case 3:
            return [3, some(value.fields[0])];
        default:
            return [0, undefined];
    }
}

function RenderFrameTransportModule_disclosureFromTransport(field, kind, value) {
    let matchResult;
    switch (kind) {
        case 0: {
            if (value == null) {
                matchResult = 0;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 1: {
            if (value == null) {
                matchResult = 1;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 2: {
            if (value == null) {
                matchResult = 2;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 3: {
            if (value != null) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        default:
            matchResult = 4;
    }
    switch (matchResult) {
        case 0:
            return Disclosure$1.NotPresent;
        case 1:
            return Disclosure$1.NotApplicable;
        case 2:
            return Disclosure$1.ExplicitlyUnknown;
        case 3:
            return new Disclosure$1(/* Disclosed */ 3, [value_2(value)]);
        default:
            throw new Exception("Invalid disclosure tag/value combination." + ((" (Parameter \'" + field) + "\')"));
    }
}

function RenderFrameTransportModule_headingFromTransport(field, kind, value) {
    let option_3, option_1;
    return RenderFrameTransportModule_disclosureFromTransport(field, kind, (option_3 = value, (option_3 != null) ? ((option_1 = HeadingRadiansModule_tryCreate(option_3), (option_1 != null) ? option_1 : (() => {
        throw new Exception("Heading must be finite." + ((" (Parameter \'" + field) + "\')"));
    })())) : undefined));
}

function RenderFrameTransportModule_factionToTransport(faction) {
    switch (faction.tag) {
        case 1:
            return [1, undefined];
        case 2:
            return [2, undefined];
        case 3:
            return [3, faction.fields[0]];
        default:
            return [0, undefined];
    }
}

function RenderFrameTransportModule_factionFromTransport(kind, id) {
    let matchResult, value_1;
    switch (kind) {
        case 0: {
            if (id == null) {
                matchResult = 0;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 1: {
            if (id == null) {
                matchResult = 1;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 2: {
            if (id == null) {
                matchResult = 2;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 3: {
            if (id != null) {
                if (!isNullOrWhiteSpace(id)) {
                    matchResult = 3;
                    value_1 = id;
                }
                else {
                    matchResult = 4;
                }
            }
            else {
                matchResult = 4;
            }
            break;
        }
        default:
            matchResult = 4;
    }
    switch (matchResult) {
        case 0:
            return FactionVisual.Human;
        case 1:
            return FactionVisual.Arcane;
        case 2:
            return FactionVisual.Neutral;
        case 3:
            return new FactionVisual(/* OtherFaction */ 3, [value_1]);
        default:
            throw new Exception("Invalid faction transport. (Parameter \'FactionKind\')");
    }
}

function RenderFrameTransportModule_unitToTransport(unit) {
    let option_1, option_3, option_5, option_7, option_9, matchValue;
    const patternInput = RenderFrameTransportModule_disclosureToTransport(unit.Health);
    const health = patternInput[1];
    const patternInput_1 = RenderFrameTransportModule_disclosureToTransport(unit.Level);
    const patternInput_2 = RenderFrameTransportModule_disclosureToTransport(unit.StanceId);
    const patternInput_3 = RenderFrameTransportModule_disclosureToTransport(unit.BodyHeading);
    const patternInput_4 = RenderFrameTransportModule_disclosureToTransport(unit.SecondaryHeading);
    const secondary = patternInput_4[1];
    const patternInput_5 = RenderFrameTransportModule_disclosureToTransport(unit.ShortLabel);
    const patternInput_6 = RenderFrameTransportModule_factionToTransport(unit.Faction);
    return new UnitVisualTransport(unit.Id, unit.AnchorColumn, unit.AnchorRow, CellExtentModule_value(unit.FootprintWidth), CellExtentModule_value(unit.FootprintDepth), UnitClassIdModule_value(unit.ClassId), patternInput_6[0], patternInput_6[1], patternInput[0], (option_1 = health, (option_1 != null) ? HealthVisualModule_remaining(option_1) : undefined), (option_3 = health, (option_3 != null) ? HealthVisualModule_maximum(option_3) : undefined), patternInput_1[0], patternInput_1[1], patternInput_2[0], patternInput_2[1], patternInput_3[0], (option_5 = patternInput_3[1], (option_5 != null) ? HeadingRadiansModule_value(option_5) : undefined), patternInput_4[0], (option_7 = secondary, (option_7 != null) ? HeadingRadiansModule_value(option_7.Radians) : undefined), (option_9 = secondary, (option_9 != null) ? ((matchValue = option_9.Source, (matchValue.tag === 2) ? 1 : ((matchValue.tag === 0) ? 2 : 0))) : undefined), patternInput_5[0], patternInput_5[1], copy(unit.StatusIds));
}

function RenderFrameTransportModule_unitFromTransport(unit) {
    let remaining, maximum, matchValue_4, matchValue_5, matchValue_6, radians, source, option_7;
    const extent = (field, value) => {
        const option_1 = CellExtentModule_tryCreate(value);
        if (option_1 != null) {
            return option_1;
        }
        else {
            throw new Exception("Footprint extent must be positive." + ((" (Parameter \'" + field) + "\')"));
        }
    };
    let health;
    const matchValue = unit.HealthKind | 0;
    const matchValue_1 = unit.HealthRemaining;
    const matchValue_2 = unit.HealthMaximum;
    let matchResult, maximum_1, remaining_1;
    switch (matchValue) {
        case 0: {
            if (matchValue_1 == null) {
                if (matchValue_2 == null) {
                    matchResult = 0;
                }
                else {
                    matchResult = 4;
                }
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 1: {
            if (matchValue_1 == null) {
                if (matchValue_2 == null) {
                    matchResult = 1;
                }
                else {
                    matchResult = 4;
                }
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 2: {
            if (matchValue_1 == null) {
                if (matchValue_2 == null) {
                    matchResult = 2;
                }
                else {
                    matchResult = 4;
                }
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 3: {
            if (matchValue_1 != null) {
                if (matchValue_2 != null) {
                    if ((remaining = (matchValue_1 | 0), (maximum = (matchValue_2 | 0), ((maximum > 0) && (remaining >= 0)) && (remaining <= maximum)))) {
                        matchResult = 3;
                        maximum_1 = matchValue_2;
                        remaining_1 = matchValue_1;
                    }
                    else {
                        matchResult = 4;
                    }
                }
                else {
                    matchResult = 4;
                }
            }
            else {
                matchResult = 4;
            }
            break;
        }
        default:
            matchResult = 4;
    }
    switch (matchResult) {
        case 0: {
            health = Disclosure$1.NotPresent;
            break;
        }
        case 1: {
            health = Disclosure$1.NotApplicable;
            break;
        }
        case 2: {
            health = Disclosure$1.ExplicitlyUnknown;
            break;
        }
        case 3: {
            let option_5;
            const option_3 = HealthVisualModule_tryCreate(remaining_1, maximum_1);
            option_5 = ((option_3 != null) ? (new Disclosure$1(/* Disclosed */ 3, [option_3])) : undefined);
            if (option_5 != null) {
                health = option_5;
            }
            else {
                throw new Exception("Invalid health bounds. (Parameter \'HealthKind\')");
            }
            break;
        }
        default:
            throw new Exception("Invalid health disclosure or bounds. (Parameter \'HealthKind\')");
    }
    return new UnitVisual(unit.Id, unit.AnchorColumn, unit.AnchorRow, extent("FootprintWidth", unit.FootprintWidth), extent("FootprintDepth", unit.FootprintDepth), UnitClassIdModule_resolve(unit.ClassId), RenderFrameTransportModule_factionFromTransport(unit.FactionKind, unit.FactionId), health, RenderFrameTransportModule_disclosureFromTransport("LevelKind", unit.LevelKind, unit.Level), RenderFrameTransportModule_disclosureFromTransport("StanceKind", unit.StanceKind, unit.StanceId), RenderFrameTransportModule_headingFromTransport("BodyHeadingKind", unit.BodyHeadingKind, unit.BodyHeadingRadians), (matchValue_4 = (unit.SecondaryHeadingKind | 0), (matchValue_5 = unit.SecondaryHeadingRadians, (matchValue_6 = unit.SecondaryHeadingSource, (matchValue_4 === 0) ? ((matchValue_5 == null) ? ((matchValue_6 == null) ? Disclosure$1.NotPresent : (() => {
        throw new Exception("A second heading requires a disclosed angle and accepted typed source. (Parameter \'SecondaryHeadingKind\')");
    })()) : (() => {
        throw new Exception("A second heading requires a disclosed angle and accepted typed source. (Parameter \'SecondaryHeadingKind\')");
    })()) : ((matchValue_4 === 1) ? ((matchValue_5 == null) ? ((matchValue_6 == null) ? Disclosure$1.NotApplicable : (() => {
        throw new Exception("A second heading requires a disclosed angle and accepted typed source. (Parameter \'SecondaryHeadingKind\')");
    })()) : (() => {
        throw new Exception("A second heading requires a disclosed angle and accepted typed source. (Parameter \'SecondaryHeadingKind\')");
    })()) : ((matchValue_4 === 2) ? ((matchValue_5 == null) ? ((matchValue_6 == null) ? Disclosure$1.ExplicitlyUnknown : (() => {
        throw new Exception("A second heading requires a disclosed angle and accepted typed source. (Parameter \'SecondaryHeadingKind\')");
    })()) : (() => {
        throw new Exception("A second heading requires a disclosed angle and accepted typed source. (Parameter \'SecondaryHeadingKind\')");
    })()) : ((matchValue_4 === 3) ? ((matchValue_5 != null) ? ((matchValue_6 != null) ? ((radians = matchValue_5, (source = (matchValue_6 | 0), new Disclosure$1(/* Disclosed */ 3, [new SecondaryHeadingVisual((option_7 = HeadingRadiansModule_tryCreate(radians), (option_7 != null) ? option_7 : (() => {
        throw new Exception("Heading must be finite. (Parameter \'SecondaryHeadingRadians\')");
    })()), (source === 0) ? SecondaryHeadingSource.WeaponHeading : ((source === 1) ? SecondaryHeadingSource.SensorHeading : ((source === 2) ? SecondaryHeadingSource.AttentionHeading : (() => {
        throw new Exception("Unknown secondary-heading gameplay source. (Parameter \'SecondaryHeadingSource\')");
    })())))])))) : (() => {
        throw new Exception("A second heading requires a disclosed angle and accepted typed source. (Parameter \'SecondaryHeadingKind\')");
    })()) : (() => {
        throw new Exception("A second heading requires a disclosed angle and accepted typed source. (Parameter \'SecondaryHeadingKind\')");
    })()) : (() => {
        throw new Exception("A second heading requires a disclosed angle and accepted typed source. (Parameter \'SecondaryHeadingKind\')");
    })())))))), RenderFrameTransportModule_disclosureFromTransport("ShortLabelKind", unit.ShortLabelKind, unit.ShortLabel), copy(unit.StatusIds));
}

export function RenderFrameTransportModule_toTransport(frame) {
    let matchValue_1;
    return new RenderFrameTransport(frame.Tick, frame.Board.MinimumColumn, frame.Board.MinimumRow, frame.Board.MaximumColumn, frame.Board.MaximumRow, map(RenderFrameTransportModule_unitToTransport, frame.Units), map((edge) => (new EdgeVisualTransport(edge.Id, edge.Kind, edge.State, edge.StartColumn, edge.StartRow, edge.EndColumn, edge.EndRow)), frame.Edges), map((overlay) => {
        const patternInput = RenderFrameTransportModule_disclosureToTransport(overlay.Label);
        let patternInput_1;
        const matchValue = overlay.Scope;
        patternInput_1 = ((matchValue.tag === 1) ? [1, undefined] : [0, matchValue.fields[0]]);
        return new OverlayVisualTransport(overlay.Id, overlay.Kind, patternInput_1[0], patternInput_1[1], overlay.GeometryRevision, copy(overlay.Points), patternInput[0], patternInput[1]);
    }, frame.Overlays), map((event) => {
        const patternInput_2 = RenderFrameTransportModule_disclosureToTransport(event.SourceUnitId);
        const patternInput_3 = RenderFrameTransportModule_disclosureToTransport(event.TargetUnitId);
        const patternInput_4 = RenderFrameTransportModule_disclosureToTransport(event.Summary);
        return new RenderEventVisualTransport(event.Id, event.Tick, event.Kind, patternInput_2[0], patternInput_2[1], patternInput_3[0], patternInput_3[1], patternInput_4[0], patternInput_4[1]);
    }, frame.Events), (matchValue_1 = frame.Disclosure, (matchValue_1.tag === 1) ? 1 : ((matchValue_1.tag === 2) ? 2 : 0)));
}

export function RenderFrameTransportModule_fromTransport(frame) {
    let matchValue_3;
    return new RenderFrame(frame.Tick, new BoardVisual(frame.BoardMinimumColumn, frame.BoardMinimumRow, frame.BoardMaximumColumn, frame.BoardMaximumRow), map(RenderFrameTransportModule_unitFromTransport, frame.Units), map((edge) => (new EdgeVisual(edge.Id, edge.Kind, edge.State, edge.StartColumn, edge.StartRow, edge.EndColumn, edge.EndRow)), frame.Edges), map((overlay) => {
        let matchValue, matchValue_1;
        return new OverlayVisual(overlay.Id, overlay.Kind, (matchValue = (overlay.ScopeKind | 0), (matchValue_1 = overlay.ScopeUnitId, (matchValue === 0) ? ((matchValue_1 != null) ? (new OverlayScope(/* SelectedUnitOverlay */ 0, [matchValue_1])) : (() => {
            throw new Exception("Invalid overlay scope tag/value combination. (Parameter \'ScopeKind\')");
        })()) : ((matchValue === 1) ? ((matchValue_1 == null) ? OverlayScope.WholeForceOverlay : (() => {
            throw new Exception("Invalid overlay scope tag/value combination. (Parameter \'ScopeKind\')");
        })()) : (() => {
            throw new Exception("Invalid overlay scope tag/value combination. (Parameter \'ScopeKind\')");
        })()))), overlay.GeometryRevision, copy(overlay.Points), RenderFrameTransportModule_disclosureFromTransport("LabelKind", overlay.LabelKind, overlay.Label));
    }, frame.Overlays), map((event) => (new RenderEventVisual(event.Id, event.Tick, event.Kind, RenderFrameTransportModule_disclosureFromTransport("SourceUnitIdKind", event.SourceUnitIdKind, event.SourceUnitId), RenderFrameTransportModule_disclosureFromTransport("TargetUnitIdKind", event.TargetUnitIdKind, event.TargetUnitId), RenderFrameTransportModule_disclosureFromTransport("SummaryKind", event.SummaryKind, event.Summary))), frame.Events), (matchValue_3 = (frame.Disclosure | 0), (matchValue_3 === 0) ? DisclosureLabel.FullReplayDisclosure : ((matchValue_3 === 1) ? DisclosureLabel.PerspectiveDisclosure : ((matchValue_3 === 2) ? DisclosureLabel.SandboxDisclosure : (() => {
        throw new Exception(("Unknown frame disclosure value: " + int32ToString(matchValue_3)) + " (Parameter \'Disclosure\')");
    })()))));
}

