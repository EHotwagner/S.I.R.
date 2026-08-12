
import { FSharpRef, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { class_type, list_type, record_type, bool_type, int32_type, string_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { singleton as singleton_1, fold, contains as contains_1, choose, append, reverse, cons, empty, item as item_1, length, findIndex, tryFind, max, isEmpty, filter, sortBy, indexed, map, ofArray } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { map as map_1, singleton, collect, delay, toList } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { stringHash, compare, int32ToString, comparePrimitives, compareArrays, equals } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { TacticalModality } from "./UnifiedTacticalWorkspace.js";
import { min, max as max_1 } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { substring, startsWith, join } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { Result_Map, FSharpResult$2, Result_Bind } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { isControl } from "../fable_modules/fable-library-js.5.13.0/Char.js";
import { tryParse } from "../fable_modules/fable-library-js.5.13.0/Int32.js";
import { tryFind as tryFind_1, toSeq, add, containsKey, empty as empty_1 } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { contains, ofList, ofSeq } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { value as value_1 } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { List_countBy } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";

export class SidebarSide extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Left", "Right"];
    }
    static Left = new SidebarSide(0, []);
    static Right = new SidebarSide(1, []);
}

export function SidebarSide_$reflection() {
    return union_type("SIR.Client.SidebarSide", [], SidebarSide, () => [[], []]);
}

export class TacticalPanelDefinition extends Record {
    constructor(Id, Label, DefaultSide, DefaultOrder, DefaultVisible, DefaultCollapsed) {
        super();
        this.Id = Id;
        this.Label = Label;
        this.DefaultSide = DefaultSide;
        this.DefaultOrder = (DefaultOrder | 0);
        this.DefaultVisible = DefaultVisible;
        this.DefaultCollapsed = DefaultCollapsed;
    }
}

export function TacticalPanelDefinition_$reflection() {
    return record_type("SIR.Client.TacticalPanelDefinition", [], TacticalPanelDefinition, () => [["Id", string_type], ["Label", string_type], ["DefaultSide", SidebarSide_$reflection()], ["DefaultOrder", int32_type], ["DefaultVisible", bool_type], ["DefaultCollapsed", bool_type]]);
}

export class PanelPlacement extends Record {
    constructor(PanelId, Side, Order, Visible, Collapsed) {
        super();
        this.PanelId = PanelId;
        this.Side = Side;
        this.Order = (Order | 0);
        this.Visible = Visible;
        this.Collapsed = Collapsed;
    }
}

export function PanelPlacement_$reflection() {
    return record_type("SIR.Client.PanelPlacement", [], PanelPlacement, () => [["PanelId", string_type], ["Side", SidebarSide_$reflection()], ["Order", int32_type], ["Visible", bool_type], ["Collapsed", bool_type]]);
}

export class SidebarLayout extends Record {
    constructor(Width, DrawerOpen) {
        super();
        this.Width = (Width | 0);
        this.DrawerOpen = DrawerOpen;
    }
}

export function SidebarLayout_$reflection() {
    return record_type("SIR.Client.SidebarLayout", [], SidebarLayout, () => [["Width", int32_type], ["DrawerOpen", bool_type]]);
}

export class BottomPanelLayout extends Record {
    constructor(Visible, Height, CollapsedInEditor, CollapsedOutsideEditor) {
        super();
        this.Visible = Visible;
        this.Height = (Height | 0);
        this.CollapsedInEditor = CollapsedInEditor;
        this.CollapsedOutsideEditor = CollapsedOutsideEditor;
    }
}

export function BottomPanelLayout_$reflection() {
    return record_type("SIR.Client.BottomPanelLayout", [], BottomPanelLayout, () => [["Visible", bool_type], ["Height", int32_type], ["CollapsedInEditor", bool_type], ["CollapsedOutsideEditor", bool_type]]);
}

export class TacticalLayoutProfile extends Record {
    constructor(SchemaVersion, Placements, LeftSidebar, RightSidebar, BottomPanel) {
        super();
        this.SchemaVersion = (SchemaVersion | 0);
        this.Placements = Placements;
        this.LeftSidebar = LeftSidebar;
        this.RightSidebar = RightSidebar;
        this.BottomPanel = BottomPanel;
    }
}

export function TacticalLayoutProfile_$reflection() {
    return record_type("SIR.Client.TacticalLayoutProfile", [], TacticalLayoutProfile, () => [["SchemaVersion", int32_type], ["Placements", list_type(PanelPlacement_$reflection())], ["LeftSidebar", SidebarLayout_$reflection()], ["RightSidebar", SidebarLayout_$reflection()], ["BottomPanel", BottomPanelLayout_$reflection()]]);
}

export class TacticalLayoutDiagnostic extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["UnknownPanel", "DuplicatePanel", "MalformedLayoutProfile", "UnsupportedLayoutSchema", "InvalidLayoutDimension"];
    }
}

export function TacticalLayoutDiagnostic_$reflection() {
    return union_type("SIR.Client.TacticalLayoutDiagnostic", [], TacticalLayoutDiagnostic, () => [[["Item", string_type]], [["Item", string_type]], [["Item", string_type]], [["Item", int32_type]], [["name", string_type], ["value", int32_type]]]);
}

export const TacticalWorkspaceLayout_panelRegistry = ofArray([new TacticalPanelDefinition("roster", "Roster / outliner", SidebarSide.Left, 0, true, false), new TacticalPanelDefinition("tools", "Tools", SidebarSide.Left, 1, true, false), new TacticalPanelDefinition("layers", "Layers", SidebarSide.Left, 2, true, true), new TacticalPanelDefinition("samples", "Samples", SidebarSide.Left, 3, false, false), new TacticalPanelDefinition("selection", "Selection inspector", SidebarSide.Right, 0, true, false), new TacticalPanelDefinition("validation", "Validation", SidebarSide.Right, 1, true, true), new TacticalPanelDefinition("document", "Document / revision", SidebarSide.Right, 2, true, true), new TacticalPanelDefinition("rules", "Rules", SidebarSide.Right, 3, false, false), new TacticalPanelDefinition("data", "Data", SidebarSide.Right, 4, false, false), new TacticalPanelDefinition("diagnostics", "Diagnostics", SidebarSide.Right, 5, false, true)]);

function TacticalWorkspaceLayout_defaultPlacement(panel) {
    return new PanelPlacement(panel.Id, panel.DefaultSide, panel.DefaultOrder, panel.DefaultVisible, panel.DefaultCollapsed);
}

export const TacticalWorkspaceLayout_fieldFocus = new TacticalLayoutProfile(1, map(TacticalWorkspaceLayout_defaultPlacement, TacticalWorkspaceLayout_panelRegistry), new SidebarLayout(208, false), new SidebarLayout(224, false), new BottomPanelLayout(true, 152, false, false));

function TacticalWorkspaceLayout_normalize(profile) {
    return new TacticalLayoutProfile(profile.SchemaVersion, toList(delay(() => collect((side) => collect((matchValue) => {
        const placement_2 = matchValue[1];
        return singleton(new PanelPlacement(placement_2.PanelId, placement_2.Side, matchValue[0], placement_2.Visible, placement_2.Collapsed));
    }, indexed(sortBy((placement_1) => [placement_1.Order, placement_1.PanelId], filter((placement) => equals(placement.Side, side), profile.Placements), {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }))), [SidebarSide.Left, SidebarSide.Right]))), profile.LeftSidebar, profile.RightSidebar, profile.BottomPanel);
}

export function TacticalWorkspaceLayout_panelsOn(side, profile) {
    return sortBy((placement_1) => [placement_1.Order, placement_1.PanelId], filter((placement) => equals(placement.Side, side), profile.Placements), {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    });
}

export function TacticalWorkspaceLayout_bottomVisible(profile) {
    return profile.BottomPanel.Visible;
}

export function TacticalWorkspaceLayout_bottomCollapsed(modality, profile) {
    if (equals(modality, TacticalModality.Editor)) {
        return profile.BottomPanel.CollapsedInEditor;
    }
    else {
        return profile.BottomPanel.CollapsedOutsideEditor;
    }
}

function TacticalWorkspaceLayout_updatePanel(panelId, change, profile) {
    return TacticalWorkspaceLayout_normalize(new TacticalLayoutProfile(profile.SchemaVersion, map((placement) => {
        if (placement.PanelId === panelId) {
            return change(placement);
        }
        else {
            return placement;
        }
    }, profile.Placements), profile.LeftSidebar, profile.RightSidebar, profile.BottomPanel));
}

export function TacticalWorkspaceLayout_togglePanelVisibility(panelId, profile) {
    return TacticalWorkspaceLayout_updatePanel(panelId, (placement) => (new PanelPlacement(placement.PanelId, placement.Side, placement.Order, !placement.Visible, placement.Collapsed)), profile);
}

export function TacticalWorkspaceLayout_togglePanelCollapsed(panelId, profile) {
    return TacticalWorkspaceLayout_updatePanel(panelId, (placement) => (new PanelPlacement(placement.PanelId, placement.Side, placement.Order, placement.Visible, !placement.Collapsed)), profile);
}

export function TacticalWorkspaceLayout_movePanel(panelId, side, profile) {
    let nextOrder;
    const _arg_1 = map((_arg) => (_arg.Order | 0), TacticalWorkspaceLayout_panelsOn(side, profile));
    nextOrder = (isEmpty(_arg_1) ? 0 : (max(_arg_1, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }) + 1));
    return TacticalWorkspaceLayout_updatePanel(panelId, (placement) => (new PanelPlacement(placement.PanelId, side, nextOrder, placement.Visible, placement.Collapsed)), profile);
}

export function TacticalWorkspaceLayout_reorderPanel(panelId, delta, profile) {
    const matchValue = tryFind((panel) => (panel.PanelId === panelId), profile.Placements);
    if (matchValue != null) {
        const selected = matchValue;
        const ordered = TacticalWorkspaceLayout_panelsOn(selected.Side, profile);
        const current = findIndex((panel_1) => (panel_1.PanelId === panelId), ordered) | 0;
        const target = max_1(0, min(length(ordered) - 1, current + delta)) | 0;
        if (target === current) {
            return profile;
        }
        else {
            const other = item_1(target, ordered);
            return TacticalWorkspaceLayout_normalize(new TacticalLayoutProfile(profile.SchemaVersion, map((panel_2) => {
                if (panel_2.PanelId === selected.PanelId) {
                    return new PanelPlacement(panel_2.PanelId, panel_2.Side, other.Order, panel_2.Visible, panel_2.Collapsed);
                }
                else if (panel_2.PanelId === other.PanelId) {
                    return new PanelPlacement(panel_2.PanelId, panel_2.Side, selected.Order, panel_2.Visible, panel_2.Collapsed);
                }
                else {
                    return panel_2;
                }
            }, profile.Placements), profile.LeftSidebar, profile.RightSidebar, profile.BottomPanel));
        }
    }
    else {
        return profile;
    }
}

export function TacticalWorkspaceLayout_toggleDrawer(side, profile) {
    if (side.tag === 1) {
        return new TacticalLayoutProfile(profile.SchemaVersion, profile.Placements, profile.LeftSidebar, new SidebarLayout(profile.RightSidebar.Width, !profile.RightSidebar.DrawerOpen), profile.BottomPanel);
    }
    else {
        return new TacticalLayoutProfile(profile.SchemaVersion, profile.Placements, new SidebarLayout(profile.LeftSidebar.Width, !profile.LeftSidebar.DrawerOpen), profile.RightSidebar, profile.BottomPanel);
    }
}

export function TacticalWorkspaceLayout_toggleBottomPanelVisibility(profile) {
    let bind$0040;
    return new TacticalLayoutProfile(profile.SchemaVersion, profile.Placements, profile.LeftSidebar, profile.RightSidebar, (bind$0040 = profile.BottomPanel, new BottomPanelLayout(!profile.BottomPanel.Visible, bind$0040.Height, bind$0040.CollapsedInEditor, bind$0040.CollapsedOutsideEditor)));
}

export function TacticalWorkspaceLayout_toggleBottomPanel(modality, profile) {
    let bind$0040, bind$0040_1;
    return new TacticalLayoutProfile(profile.SchemaVersion, profile.Placements, profile.LeftSidebar, profile.RightSidebar, equals(modality, TacticalModality.Editor) ? ((bind$0040 = profile.BottomPanel, new BottomPanelLayout(bind$0040.Visible, bind$0040.Height, !profile.BottomPanel.CollapsedInEditor, bind$0040.CollapsedOutsideEditor))) : ((bind$0040_1 = profile.BottomPanel, new BottomPanelLayout(bind$0040_1.Visible, bind$0040_1.Height, bind$0040_1.CollapsedInEditor, !profile.BottomPanel.CollapsedOutsideEditor))));
}

export function TacticalWorkspaceLayout_resizeBottomPanel(height, profile) {
    let bind$0040;
    return new TacticalLayoutProfile(profile.SchemaVersion, profile.Placements, profile.LeftSidebar, profile.RightSidebar, (bind$0040 = profile.BottomPanel, new BottomPanelLayout(bind$0040.Visible, max_1(96, min(480, height)), bind$0040.CollapsedInEditor, bind$0040.CollapsedOutsideEditor)));
}

export function TacticalWorkspaceLayout_reset(_arg) {
    return TacticalWorkspaceLayout_fieldFocus;
}

function TacticalWorkspaceLayout_sideText(_arg) {
    if (_arg.tag === 1) {
        return "right";
    }
    else {
        return "left";
    }
}

function TacticalWorkspaceLayout_boolText(value) {
    if (value) {
        return "true";
    }
    else {
        return "false";
    }
}

function TacticalWorkspaceLayout_placementJson(placement) {
    return ((((((((("{\"panelId\":\"" + placement.PanelId) + "\",\"side\":\"") + TacticalWorkspaceLayout_sideText(placement.Side)) + "\",\"order\":") + int32ToString(placement.Order)) + ",\"visible\":") + TacticalWorkspaceLayout_boolText(placement.Visible)) + ",\"collapsed\":") + TacticalWorkspaceLayout_boolText(placement.Collapsed)) + "}";
}

function TacticalWorkspaceLayout_sidebarJson(sidebar) {
    return ((("{\"width\":" + int32ToString(sidebar.Width)) + ",\"drawerOpen\":") + TacticalWorkspaceLayout_boolText(sidebar.DrawerOpen)) + "}";
}

function TacticalWorkspaceLayout_bottomJson(bottom) {
    return ((((((("{\"visible\":" + TacticalWorkspaceLayout_boolText(bottom.Visible)) + ",\"height\":") + int32ToString(bottom.Height)) + ",\"collapsedInEditor\":") + TacticalWorkspaceLayout_boolText(bottom.CollapsedInEditor)) + ",\"collapsedOutsideEditor\":") + TacticalWorkspaceLayout_boolText(bottom.CollapsedOutsideEditor)) + "}";
}

export function TacticalWorkspaceLayout_exportProfile(profile) {
    const normalized = TacticalWorkspaceLayout_normalize(profile);
    return ((((((("{\"schemaVersion\":1,\"placements\":[" + join(",", map(TacticalWorkspaceLayout_placementJson, sortBy((placement) => [placement.Side, placement.Order, placement.PanelId], normalized.Placements, {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    })))) + "],\"leftSidebar\":") + TacticalWorkspaceLayout_sidebarJson(normalized.LeftSidebar)) + ",\"rightSidebar\":") + TacticalWorkspaceLayout_sidebarJson(normalized.RightSidebar)) + ",\"bottomPanel\":") + TacticalWorkspaceLayout_bottomJson(normalized.BottomPanel)) + "}";
}

class TacticalWorkspaceLayout_Json extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Null", "String", "Number", "Boolean", "Array", "Object"];
    }
    static Null = new TacticalWorkspaceLayout_Json(0, []);
}

function TacticalWorkspaceLayout_Json_$reflection() {
    return union_type("SIR.Client.TacticalWorkspaceLayout.Json", [], TacticalWorkspaceLayout_Json, () => [[], [["Item", string_type]], [["Item", int32_type]], [["Item", bool_type]], [["Item", list_type(TacticalWorkspaceLayout_Json_$reflection())]], [["Item", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, TacticalWorkspaceLayout_Json_$reflection()])]]]);
}

class TacticalWorkspaceLayout_ResultBuilder {
    constructor() {
    }
}

function TacticalWorkspaceLayout_ResultBuilder_$reflection() {
    return class_type("SIR.Client.TacticalWorkspaceLayout.ResultBuilder", undefined, TacticalWorkspaceLayout_ResultBuilder);
}

function TacticalWorkspaceLayout_ResultBuilder_$ctor() {
    return new TacticalWorkspaceLayout_ResultBuilder();
}

function TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(_, value, binder) {
    return Result_Bind(binder, value);
}

function TacticalWorkspaceLayout_ResultBuilder__Return_1505(_, value) {
    return new FSharpResult$2(/* Ok */ 0, [value]);
}

function TacticalWorkspaceLayout_ResultBuilder__ReturnFrom_1505(_, value) {
    return value;
}

const TacticalWorkspaceLayout_result = TacticalWorkspaceLayout_ResultBuilder_$ctor();

function TacticalWorkspaceLayout_parseJson(source) {
    let index = 0;
    const malformed = (detail) => (new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, [detail])]));
    const isAsciiDigit = (character) => {
        if (character >= "0") {
            return character <= "9";
        }
        else {
            return false;
        }
    };
    const skip = () => {
        let _arg;
        while ((index < source.length) && ((_arg = source[index], (_arg === "\t") ? true : ((_arg === "\n") ? true : ((_arg === "\r") ? true : (_arg === " ")))))) {
            index = ((index + 1) | 0);
        }
    };
    const value = () => {
        let token, token_1, token_2, token_3;
        skip();
        if (index >= source.length) {
            return malformed("Unexpected end of JSON.");
        }
        else {
            const matchValue = source[index];
            let matchResult, token_4, token_5;
            switch (matchValue) {
                case "\"": {
                    matchResult = 0;
                    break;
                }
                case "[": {
                    matchResult = 2;
                    break;
                }
                case "f": {
                    if (startsWith(substring(source, index), "false", 4)) {
                        matchResult = 4;
                    }
                    else if ((token = matchValue, (token === "-") ? true : isAsciiDigit(token))) {
                        matchResult = 6;
                        token_4 = matchValue;
                    }
                    else {
                        matchResult = 7;
                        token_5 = matchValue;
                    }
                    break;
                }
                case "n": {
                    if (startsWith(substring(source, index), "null", 4)) {
                        matchResult = 5;
                    }
                    else if ((token_1 = matchValue, (token_1 === "-") ? true : isAsciiDigit(token_1))) {
                        matchResult = 6;
                        token_4 = matchValue;
                    }
                    else {
                        matchResult = 7;
                        token_5 = matchValue;
                    }
                    break;
                }
                case "t": {
                    if (startsWith(substring(source, index), "true", 4)) {
                        matchResult = 3;
                    }
                    else if ((token_2 = matchValue, (token_2 === "-") ? true : isAsciiDigit(token_2))) {
                        matchResult = 6;
                        token_4 = matchValue;
                    }
                    else {
                        matchResult = 7;
                        token_5 = matchValue;
                    }
                    break;
                }
                case "{": {
                    matchResult = 1;
                    break;
                }
                default:
                    if ((token_3 = matchValue, (token_3 === "-") ? true : isAsciiDigit(token_3))) {
                        matchResult = 6;
                        token_4 = matchValue;
                    }
                    else {
                        matchResult = 7;
                        token_5 = matchValue;
                    }
            }
            switch (matchResult) {
                case 0:
                    return Result_Map((Item) => (new TacticalWorkspaceLayout_Json(/* String */ 1, [Item])), stringValue());
                case 1:
                    return objectValue();
                case 2:
                    return arrayValue();
                case 3: {
                    index = ((index + 4) | 0);
                    return new FSharpResult$2(/* Ok */ 0, [new TacticalWorkspaceLayout_Json(/* Boolean */ 3, [true])]);
                }
                case 4: {
                    index = ((index + 5) | 0);
                    return new FSharpResult$2(/* Ok */ 0, [new TacticalWorkspaceLayout_Json(/* Boolean */ 3, [false])]);
                }
                case 5: {
                    index = ((index + 4) | 0);
                    return new FSharpResult$2(/* Ok */ 0, [TacticalWorkspaceLayout_Json.Null]);
                }
                case 6:
                    return Result_Map((Item_1) => (new TacticalWorkspaceLayout_Json(/* Number */ 2, [Item_1])), numberValue());
                default:
                    return malformed(("Unexpected JSON token " + token_5) + ".");
            }
        }
    };
    const stringValue = () => {
        index = ((index + 1) | 0);
        let parsed = "";
        let done$0027 = false;
        let error = undefined;
        while (((index < source.length) && !done$0027) && (error == null)) {
            const matchValue_1 = source[index];
            switch (matchValue_1) {
                case "\"": {
                    done$0027 = true;
                    index = ((index + 1) | 0);
                    break;
                }
                case "\\": {
                    index = ((index + 1) | 0);
                    if (index >= source.length) {
                        error = "Incomplete JSON escape.";
                    }
                    else {
                        const matchValue_2 = source[index];
                        let matchResult_1, escaped;
                        switch (matchValue_2) {
                            case "\"": {
                                matchResult_1 = 0;
                                escaped = matchValue_2;
                                break;
                            }
                            case "/": {
                                matchResult_1 = 0;
                                escaped = matchValue_2;
                                break;
                            }
                            case "\\": {
                                matchResult_1 = 0;
                                escaped = matchValue_2;
                                break;
                            }
                            default:
                                matchResult_1 = 1;
                        }
                        switch (matchResult_1) {
                            case 0: {
                                parsed = (parsed + escaped);
                                index = ((index + 1) | 0);
                                break;
                            }
                            case 1: {
                                error = "Unsupported JSON escape.";
                                break;
                            }
                        }
                    }
                    break;
                }
                default:
                    if (isControl(matchValue_1)) {
                        error = "Control character in JSON string.";
                    }
                    else {
                        parsed = (parsed + matchValue_1);
                        index = ((index + 1) | 0);
                    }
            }
        }
        if (error == null) {
            if (!done$0027) {
                return malformed("Unterminated JSON string.");
            }
            else {
                return new FSharpResult$2(/* Ok */ 0, [parsed]);
            }
        }
        else {
            return malformed(error);
        }
    };
    const numberValue = () => {
        const start = index | 0;
        if (source[index] === "-") {
            index = ((index + 1) | 0);
        }
        if ((index >= source.length) ? true : !isAsciiDigit(source[index])) {
            return malformed("JSON integer requires an ASCII digit after its optional minus.");
        }
        else if (source[index] === "0") {
            index = ((index + 1) | 0);
            if ((index < source.length) && isAsciiDigit(source[index])) {
                return malformed("JSON integer cannot contain a leading zero.");
            }
            else {
                let matchValue_3;
                let outArg = 0;
                matchValue_3 = [tryParse(substring(source, start, index - start), 511, false, 32, new FSharpRef(() => (outArg | 0), (v) => {
                    outArg = (v | 0);
                })), outArg];
                if (matchValue_3[0]) {
                    return new FSharpResult$2(/* Ok */ 0, [matchValue_3[1]]);
                }
                else {
                    return malformed("JSON number must be a 32-bit integer.");
                }
            }
        }
        else {
            while ((index < source.length) && isAsciiDigit(source[index])) {
                index = ((index + 1) | 0);
            }
            let matchValue_4;
            let outArg_1 = 0;
            matchValue_4 = [tryParse(substring(source, start, index - start), 511, false, 32, new FSharpRef(() => (outArg_1 | 0), (v_1) => {
                outArg_1 = (v_1 | 0);
            })), outArg_1];
            if (matchValue_4[0]) {
                return new FSharpResult$2(/* Ok */ 0, [matchValue_4[1]]);
            }
            else {
                return malformed("JSON number must be a 32-bit integer.");
            }
        }
    };
    const arrayValue = () => {
        index = ((index + 1) | 0);
        skip();
        let items = empty();
        let done$0027_1 = false;
        let afterComma = false;
        let error_1 = undefined;
        while (!done$0027_1 && (error_1 == null)) {
            skip();
            if ((index < source.length) && (source[index] === "]")) {
                if (afterComma) {
                    error_1 = (new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Trailing comma in JSON array."]));
                }
                else {
                    done$0027_1 = true;
                    index = ((index + 1) | 0);
                }
            }
            else {
                const matchValue_5 = value();
                if (matchValue_5.tag === 0) {
                    afterComma = false;
                    items = cons(matchValue_5.fields[0], items);
                    skip();
                    if ((index < source.length) && (source[index] === ",")) {
                        index = ((index + 1) | 0);
                        afterComma = true;
                    }
                    else if ((index < source.length) && (source[index] === "]")) {
                    }
                    else {
                        error_1 = (new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Expected \',\' or \']\'."]));
                    }
                }
                else {
                    error_1 = matchValue_5.fields[0];
                }
            }
        }
        if (error_1 == null) {
            return new FSharpResult$2(/* Ok */ 0, [new TacticalWorkspaceLayout_Json(/* Array */ 4, [reverse(items)])]);
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [error_1]);
        }
    };
    const objectValue = () => {
        index = ((index + 1) | 0);
        skip();
        let fields = empty_1({
            Compare: (x, y) => (comparePrimitives(x, y) | 0),
        });
        let done$0027_2 = false;
        let afterComma_1 = false;
        let error_2 = undefined;
        while (!done$0027_2 && (error_2 == null)) {
            skip();
            if ((index < source.length) && (source[index] === "}")) {
                if (afterComma_1) {
                    error_2 = (new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Trailing comma in JSON object."]));
                }
                else {
                    done$0027_2 = true;
                    index = ((index + 1) | 0);
                }
            }
            else if ((index >= source.length) ? true : (source[index] !== "\"")) {
                error_2 = (new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Expected JSON object field name."]));
            }
            else {
                const matchValue_6 = stringValue();
                if (matchValue_6.tag === 0) {
                    const name = matchValue_6.fields[0];
                    skip();
                    if ((index >= source.length) ? true : (source[index] !== ":")) {
                        error_2 = (new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Expected \':\'."]));
                    }
                    else if (containsKey(name, fields)) {
                        error_2 = (new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, [("Duplicate field " + name) + "."]));
                    }
                    else {
                        index = ((index + 1) | 0);
                        const matchValue_7 = value();
                        if (matchValue_7.tag === 0) {
                            afterComma_1 = false;
                            fields = add(name, matchValue_7.fields[0], fields);
                            skip();
                            if ((index < source.length) && (source[index] === ",")) {
                                index = ((index + 1) | 0);
                                afterComma_1 = true;
                            }
                            else if ((index < source.length) && (source[index] === "}")) {
                            }
                            else {
                                error_2 = (new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Expected \',\' or \'}\'."]));
                            }
                        }
                        else {
                            error_2 = matchValue_7.fields[0];
                        }
                    }
                }
                else {
                    error_2 = matchValue_6.fields[0];
                }
            }
        }
        if (error_2 == null) {
            return new FSharpResult$2(/* Ok */ 0, [new TacticalWorkspaceLayout_Json(/* Object */ 5, [fields])]);
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [error_2]);
        }
    };
    const matchValue_8 = value();
    if (matchValue_8.tag === 0) {
        skip();
        if (index !== source.length) {
            return malformed("Trailing JSON content.");
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [matchValue_8.fields[0]]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [matchValue_8.fields[0]]);
    }
}

function TacticalWorkspaceLayout_exactFields(expected, fields) {
    const actual = ofSeq(map_1((tuple) => tuple[0], toSeq(fields)), {
        Compare: (x, y) => (compare(x, y) | 0),
    });
    if (actual.Equals(ofList(expected, {
        Compare: (x_1, y_1) => (compare(x_1, y_1) | 0),
    }))) {
        return new FSharpResult$2(/* Ok */ 0, [fields]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Missing or unknown layout field."])]);
    }
}

function TacticalWorkspaceLayout_field(name, fields) {
    const matchValue = tryFind_1(name, fields);
    if (matchValue == null) {
        return new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, [("Missing field " + name) + "."])]);
    }
    else {
        return new FSharpResult$2(/* Ok */ 0, [value_1(matchValue)]);
    }
}

function TacticalWorkspaceLayout_asObject(_arg) {
    if (_arg.tag === 5) {
        return new FSharpResult$2(/* Ok */ 0, [_arg.fields[0]]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Expected object."])]);
    }
}

function TacticalWorkspaceLayout_asArray(_arg) {
    if (_arg.tag === 4) {
        return new FSharpResult$2(/* Ok */ 0, [_arg.fields[0]]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Expected array."])]);
    }
}

function TacticalWorkspaceLayout_asString(_arg) {
    if (_arg.tag === 1) {
        return new FSharpResult$2(/* Ok */ 0, [_arg.fields[0]]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Expected string."])]);
    }
}

function TacticalWorkspaceLayout_asNumber(_arg) {
    if (_arg.tag === 2) {
        return new FSharpResult$2(/* Ok */ 0, [_arg.fields[0]]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Expected number."])]);
    }
}

function TacticalWorkspaceLayout_asBool(_arg) {
    if (_arg.tag === 3) {
        return new FSharpResult$2(/* Ok */ 0, [_arg.fields[0]]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Expected boolean."])]);
    }
}

function TacticalWorkspaceLayout_parseSidebar(json) {
    const builder$0040 = TacticalWorkspaceLayout_result;
    return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, TacticalWorkspaceLayout_asObject(json), (_arg) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, TacticalWorkspaceLayout_exactFields(ofArray(["width", "drawerOpen"]), _arg), (_arg_1) => {
        const fields_1 = _arg_1;
        return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asNumber, TacticalWorkspaceLayout_field("width", fields_1)), (_arg_3) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asBool, TacticalWorkspaceLayout_field("drawerOpen", fields_1)), (_arg_5) => TacticalWorkspaceLayout_ResultBuilder__Return_1505(builder$0040, new SidebarLayout(_arg_3, _arg_5))));
    }));
}

function TacticalWorkspaceLayout_parseBottom(json) {
    const builder$0040 = TacticalWorkspaceLayout_result;
    return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, TacticalWorkspaceLayout_asObject(json), (_arg) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, TacticalWorkspaceLayout_exactFields(ofArray(["visible", "height", "collapsedInEditor", "collapsedOutsideEditor"]), _arg), (_arg_1) => {
        const fields_1 = _arg_1;
        return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asBool, TacticalWorkspaceLayout_field("visible", fields_1)), (_arg_3) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asNumber, TacticalWorkspaceLayout_field("height", fields_1)), (_arg_5) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asBool, TacticalWorkspaceLayout_field("collapsedInEditor", fields_1)), (_arg_7) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asBool, TacticalWorkspaceLayout_field("collapsedOutsideEditor", fields_1)), (_arg_9) => TacticalWorkspaceLayout_ResultBuilder__Return_1505(builder$0040, new BottomPanelLayout(_arg_3, _arg_5, _arg_7, _arg_9))))));
    }));
}

function TacticalWorkspaceLayout_parsePlacement(json) {
    const builder$0040 = TacticalWorkspaceLayout_result;
    return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, TacticalWorkspaceLayout_asObject(json), (_arg) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, TacticalWorkspaceLayout_exactFields(ofArray(["panelId", "side", "order", "visible", "collapsed"]), _arg), (_arg_1) => {
        const fields_1 = _arg_1;
        return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asString, TacticalWorkspaceLayout_field("panelId", fields_1)), (_arg_3) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asString, TacticalWorkspaceLayout_field("side", fields_1)), (_arg_5) => {
            const sideText = _arg_5;
            return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, (sideText === "left") ? (new FSharpResult$2(/* Ok */ 0, [SidebarSide.Left])) : ((sideText === "right") ? (new FSharpResult$2(/* Ok */ 0, [SidebarSide.Right])) : (new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* MalformedLayoutProfile */ 2, ["Panel side must be left or right."])]))), (_arg_6) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asNumber, TacticalWorkspaceLayout_field("order", fields_1)), (_arg_8) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asBool, TacticalWorkspaceLayout_field("visible", fields_1)), (_arg_10) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asBool, TacticalWorkspaceLayout_field("collapsed", fields_1)), (_arg_12) => TacticalWorkspaceLayout_ResultBuilder__Return_1505(builder$0040, new PanelPlacement(_arg_3, _arg_6, _arg_8, _arg_10, _arg_12))))));
        }));
    }));
}

function TacticalWorkspaceLayout_validate(profile) {
    const known = ofList(map((_arg) => _arg.Id, TacticalWorkspaceLayout_panelRegistry), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const ids = map((_arg_1) => _arg_1.PanelId, profile.Placements);
    const diagnostics = append(choose((tupledArg) => {
        if (tupledArg[1] > 1) {
            return new TacticalLayoutDiagnostic(/* DuplicatePanel */ 1, [tupledArg[0]]);
        }
        else {
            return undefined;
        }
    }, List_countBy((x_1) => x_1, ids, {
        Equals: (x_2, y_1) => (x_2 === y_1),
        GetHashCode: (x_2) => (stringHash(x_2) | 0),
    })), append(choose((id_1) => {
        if (contains(id_1, known)) {
            return undefined;
        }
        else {
            return new TacticalLayoutDiagnostic(/* UnknownPanel */ 0, [id_1]);
        }
    }, ids), choose((tupledArg_1) => {
        const value = tupledArg_1[1] | 0;
        if ((value < tupledArg_1[2]) ? true : (value > tupledArg_1[3])) {
            return new TacticalLayoutDiagnostic(/* InvalidLayoutDimension */ 4, [tupledArg_1[0], value]);
        }
        else {
            return undefined;
        }
    }, ofArray([["left width", profile.LeftSidebar.Width, 160, 480], ["right width", profile.RightSidebar.Width, 160, 480], ["bottom height", profile.BottomPanel.Height, 96, 480]]))));
    if (isEmpty(diagnostics)) {
        return new FSharpResult$2(/* Ok */ 0, [TacticalWorkspaceLayout_normalize(new TacticalLayoutProfile(profile.SchemaVersion, append(profile.Placements, map(TacticalWorkspaceLayout_defaultPlacement, filter((panel) => !contains_1(panel.Id, ids, {
            Equals: (x_3, y_2) => (x_3 === y_2),
            GetHashCode: (x_3) => (stringHash(x_3) | 0),
        }), TacticalWorkspaceLayout_panelRegistry))), profile.LeftSidebar, profile.RightSidebar, profile.BottomPanel))]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [diagnostics]);
    }
}

function TacticalWorkspaceLayout_current(fields) {
    const builder$0040 = TacticalWorkspaceLayout_result;
    return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, TacticalWorkspaceLayout_exactFields(ofArray(["schemaVersion", "placements", "leftSidebar", "rightSidebar", "bottomPanel"]), fields), (_arg) => {
        const fields_1 = _arg;
        return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asArray, TacticalWorkspaceLayout_field("placements", fields_1)), (_arg_2) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Map(reverse, fold((state, item) => {
            const builder$0040_1 = TacticalWorkspaceLayout_result;
            return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040_1, state, (_arg_3) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040_1, item, (_arg_4) => TacticalWorkspaceLayout_ResultBuilder__Return_1505(builder$0040_1, cons(_arg_4, _arg_3))));
        }, new FSharpResult$2(/* Ok */ 0, [empty()]), map(TacticalWorkspaceLayout_parsePlacement, _arg_2))), (_arg_5) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_parseSidebar, TacticalWorkspaceLayout_field("leftSidebar", fields_1)), (_arg_6) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_parseSidebar, TacticalWorkspaceLayout_field("rightSidebar", fields_1)), (_arg_7) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_parseBottom, TacticalWorkspaceLayout_field("bottomPanel", fields_1)), (_arg_8) => TacticalWorkspaceLayout_ResultBuilder__Return_1505(builder$0040, new TacticalLayoutProfile(1, _arg_5, _arg_6, _arg_7, _arg_8)))))));
    });
}

function TacticalWorkspaceLayout_migrateZero(fields) {
    const builder$0040 = TacticalWorkspaceLayout_result;
    return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, TacticalWorkspaceLayout_exactFields(ofArray(["schemaVersion", "panels", "leftWidth", "rightWidth", "timelineHeight"]), fields), (_arg) => {
        const fields_1 = _arg;
        return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asArray, TacticalWorkspaceLayout_field("panels", fields_1)), (_arg_2) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Map(reverse, fold((state, item) => {
            const builder$0040_1 = TacticalWorkspaceLayout_result;
            return TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040_1, state, (_arg_3) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040_1, item, (_arg_4) => TacticalWorkspaceLayout_ResultBuilder__Return_1505(builder$0040_1, cons(_arg_4, _arg_3))));
        }, new FSharpResult$2(/* Ok */ 0, [empty()]), map(TacticalWorkspaceLayout_parsePlacement, _arg_2))), (_arg_5) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asNumber, TacticalWorkspaceLayout_field("leftWidth", fields_1)), (_arg_7) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asNumber, TacticalWorkspaceLayout_field("rightWidth", fields_1)), (_arg_9) => TacticalWorkspaceLayout_ResultBuilder__Bind_764BA1D3(builder$0040, Result_Bind(TacticalWorkspaceLayout_asNumber, TacticalWorkspaceLayout_field("timelineHeight", fields_1)), (_arg_11) => {
            let bind$0040_2;
            return TacticalWorkspaceLayout_ResultBuilder__Return_1505(builder$0040, new TacticalLayoutProfile(TacticalWorkspaceLayout_fieldFocus.SchemaVersion, _arg_5, new SidebarLayout(_arg_7, TacticalWorkspaceLayout_fieldFocus.LeftSidebar.DrawerOpen), new SidebarLayout(_arg_9, TacticalWorkspaceLayout_fieldFocus.RightSidebar.DrawerOpen), (bind$0040_2 = TacticalWorkspaceLayout_fieldFocus.BottomPanel, new BottomPanelLayout(bind$0040_2.Visible, _arg_11, bind$0040_2.CollapsedInEditor, bind$0040_2.CollapsedOutsideEditor))));
        })))));
    });
}

export function TacticalWorkspaceLayout_importProfile(source) {
    const matchValue = TacticalWorkspaceLayout_parseJson(source);
    if (matchValue.tag === 0) {
        const matchValue_1 = TacticalWorkspaceLayout_asObject(matchValue.fields[0]);
        if (matchValue_1.tag === 0) {
            const fields = matchValue_1.fields[0];
            const matchValue_2 = Result_Bind(TacticalWorkspaceLayout_asNumber, TacticalWorkspaceLayout_field("schemaVersion", fields));
            if (matchValue_2.tag === 0) {
                const version = matchValue_2.fields[0] | 0;
                const parsed = (version === 0) ? TacticalWorkspaceLayout_migrateZero(fields) : ((version === 1) ? TacticalWorkspaceLayout_current(fields) : (new FSharpResult$2(/* Error */ 1, [new TacticalLayoutDiagnostic(/* UnsupportedLayoutSchema */ 3, [version])])));
                if (parsed.tag === 0) {
                    return TacticalWorkspaceLayout_validate(parsed.fields[0]);
                }
                else {
                    return new FSharpResult$2(/* Error */ 1, [singleton_1(parsed.fields[0])]);
                }
            }
            else {
                return new FSharpResult$2(/* Error */ 1, [singleton_1(matchValue_2.fields[0])]);
            }
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [singleton_1(matchValue_1.fields[0])]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [singleton_1(matchValue.fields[0])]);
    }
}

