
import { toString, Union, Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { lambda_type, list_type, class_type, int32_type, union_type, bool_type, record_type, option_type, string_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { join, substring, startsWith, isNullOrWhiteSpace, isNullOrEmpty } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { RevisionState, MapController, MapTerrain, RegionShape, MapSide, RegionPurpose, MapEdgeDirection, MapEdgeKind, EditorBox, EditorCellAddress, EditorGesture, TerrainAuthoringTool, EditorDomain, MapEditorTool, MapEditorAction, MapEditorState_$reflection, MapController_$reflection, MapEditorAction_$reflection, MapEditorTool_$reflection, EditorDomain_$reflection, TerrainAuthoringTool_$reflection } from "./MapEditorTypes.js";
import { EditorWorkspaceAction, EditorWorkspaceAction_$reflection } from "./MapEditorWorkspace.js";
import { SimulatorCollision, MapEditorSimulator_preview, SimulatorAction, SimulatorAction_$reflection } from "./MapEditorSimulator.js";
import { FSharpSet__get_Count, FSharpSet__get_IsEmpty, ofList, remove, add, contains, empty } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { stringHash, int64ToString, int32ToString, min, max, curry3, comparePrimitives, compareArrays, safeHash, equals, compare } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { indexed, skip, collect as collect_1, choose, tail, tryHead, length, append as append_1, ofArray, tryFind, map, forAll, takeWhile, head, isEmpty, filter, sortBy, empty as empty_1, singleton, contains as contains_1, exists } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { collect, empty as empty_2, singleton as singleton_1, append, delay, toList } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { op_UnaryNegation_Int32 } from "../fable_modules/fable-library-js.5.13.0/Int32.js";
import { value as value_26, defaultArg, some } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { toArray, isEmpty as isEmpty_1, tryFind as tryFind_1, FSharpMap__get_IsEmpty } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { min as min_1, max as max_1 } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { item, tryFindIndex, map as map_1, sort } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { controllerLabel, keyboardObjectsAtCursor, unitPlacementIssue, selectedUnitPalettePreset, searchCanonicalUnitPresets, terrainPreview } from "./MapEditor.js";
import { List_distinct, List_groupBy } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";

/**
 * A layout-sensitive key value normalized from KeyboardEvent.key. The
 * optional physical code is retained for diagnostics, not binding identity.
 */
export class NormalizedKey extends Record {
    constructor(Value, PhysicalCode) {
        super();
        this.Value = Value;
        this.PhysicalCode = PhysicalCode;
    }
}

export function NormalizedKey_$reflection() {
    return record_type("SIR.Client.NormalizedKey", [], NormalizedKey, () => [["Value", string_type], ["PhysicalCode", option_type(string_type)]]);
}

export function NormalizedKeyModule_create(key, physicalCode) {
    let option_1, value_4;
    return new NormalizedKey(isNullOrEmpty(key) ? "" : ((key === " ") ? "Space" : ((key === "Esc") ? "Escape" : ((key.length === 1) ? key.toLowerCase() : key))), (option_1 = physicalCode, (option_1 != null) ? ((value_4 = option_1, isNullOrWhiteSpace(value_4) ? undefined : value_4)) : undefined));
}

export function NormalizedKeyModule_value(key) {
    return key.Value;
}

export function NormalizedKeyModule_physicalCode(key) {
    return key.PhysicalCode;
}

export function NormalizedKeyModule_sameProducedKey(left, right) {
    return left.Value === right.Value;
}

export class KeyModifiers extends Record {
    constructor(ControlOrMeta, Shift, Alt) {
        super();
        this.ControlOrMeta = ControlOrMeta;
        this.Shift = Shift;
        this.Alt = Alt;
    }
}

export function KeyModifiers_$reflection() {
    return record_type("SIR.Client.KeyModifiers", [], KeyModifiers, () => [["ControlOrMeta", bool_type], ["Shift", bool_type], ["Alt", bool_type]]);
}

export const KeyModifiersModule_none = new KeyModifiers(false, false, false);

export class InputPhase extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["KeyDown", "KeyUp"];
    }
    static KeyDown = new InputPhase(0, []);
    static KeyUp = new InputPhase(1, []);
}

export function InputPhase_$reflection() {
    return union_type("SIR.Client.InputPhase", [], InputPhase, () => [[], []]);
}

/**
 * Browser targets that define the native-editing boundary before modal
 * keyboard resolution.
 */
export class ModalInputTarget extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["InputElement", "TextAreaElement", "SelectElement", "ContentEditableElement", "ApplicationElement"];
    }
    static InputElement = new ModalInputTarget(0, []);
    static TextAreaElement = new ModalInputTarget(1, []);
    static SelectElement = new ModalInputTarget(2, []);
    static ContentEditableElement = new ModalInputTarget(3, []);
    static ApplicationElement = new ModalInputTarget(4, []);
}

export function ModalInputTarget_$reflection() {
    return union_type("SIR.Client.ModalInputTarget", [], ModalInputTarget, () => [[], [], [], [], []]);
}

export class RepeatPolicy extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["IgnoreRepeat", "AllowRepeat"];
    }
    static IgnoreRepeat = new RepeatPolicy(0, []);
    static AllowRepeat = new RepeatPolicy(1, []);
}

export function RepeatPolicy_$reflection() {
    return union_type("SIR.Client.RepeatPolicy", [], RepeatPolicy, () => [[], []]);
}

export class InputGesture extends Record {
    constructor(Key, Modifiers, Phase) {
        super();
        this.Key = Key;
        this.Modifiers = Modifiers;
        this.Phase = Phase;
    }
}

export function InputGesture_$reflection() {
    return record_type("SIR.Client.InputGesture", [], InputGesture, () => [["Key", NormalizedKey_$reflection()], ["Modifiers", KeyModifiers_$reflection()], ["Phase", InputPhase_$reflection()]]);
}

export class EditorGestureKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["SelectedObjectActions", "BoxSelection", "CommandPreview", "UnitMovePreview", "TerrainPreview", "EdgePolyline", "RegionPurpose", "RegionShape", "RegionRectangleMode", "RegionPolygonMode", "RegionMove", "RegionResize", "RegionVertex"];
    }
    static SelectedObjectActions = new EditorGestureKind(0, []);
    static BoxSelection = new EditorGestureKind(1, []);
    static CommandPreview = new EditorGestureKind(2, []);
    static UnitMovePreview = new EditorGestureKind(3, []);
    static EdgePolyline = new EditorGestureKind(5, []);
    static RegionPurpose = new EditorGestureKind(6, []);
    static RegionShape = new EditorGestureKind(7, []);
    static RegionRectangleMode = new EditorGestureKind(8, []);
    static RegionPolygonMode = new EditorGestureKind(9, []);
    static RegionMove = new EditorGestureKind(10, []);
    static RegionResize = new EditorGestureKind(11, []);
    static RegionVertex = new EditorGestureKind(12, []);
}

export function EditorGestureKind_$reflection() {
    return union_type("SIR.Client.EditorGestureKind", [], EditorGestureKind, () => [[], [], [], [], [["Item", TerrainAuthoringTool_$reflection()]], [], [], [], [], [], [], [], []]);
}

export class EditorDocumentControl extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["MapImportControl", "LayerStateControls", "LocalBackgroundControls", "MapDimensionControls", "SavedViewControls"];
    }
    static MapImportControl = new EditorDocumentControl(0, []);
    static LayerStateControls = new EditorDocumentControl(1, []);
    static LocalBackgroundControls = new EditorDocumentControl(2, []);
    static MapDimensionControls = new EditorDocumentControl(3, []);
    static SavedViewControls = new EditorDocumentControl(4, []);
}

export function EditorDocumentControl_$reflection() {
    return union_type("SIR.Client.EditorDocumentControl", [], EditorDocumentControl, () => [[], [], [], [], []]);
}

export class EditorDocumentCommand extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ExportMapDocument", "OpenMapImport", "ExportRepositoryDesignBundle", "FocusDocumentControl"];
    }
    static ExportMapDocument = new EditorDocumentCommand(0, []);
    static OpenMapImport = new EditorDocumentCommand(1, []);
    static ExportRepositoryDesignBundle = new EditorDocumentCommand(2, []);
}

export function EditorDocumentCommand_$reflection() {
    return union_type("SIR.Client.EditorDocumentCommand", [], EditorDocumentCommand, () => [[], [], [], [["Item", EditorDocumentControl_$reflection()]]]);
}

export class ModalContext extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["EditorBase", "EditorDomain", "EditorTool", "EditorGesture", "EditorPanHeld", "EditorDestructiveConfirmation", "SimulatorBase", "SimulatorPaused", "SimulatorRunning", "SimulatorRoutePreview", "SimulatorControllerSelection", "SimulatorRevisionStale", "SimulatorNoHandoff", "InputHelpPopup"];
    }
    static EditorBase = new ModalContext(0, []);
    static EditorPanHeld = new ModalContext(4, []);
    static EditorDestructiveConfirmation = new ModalContext(5, []);
    static SimulatorBase = new ModalContext(6, []);
    static SimulatorPaused = new ModalContext(7, []);
    static SimulatorRunning = new ModalContext(8, []);
    static SimulatorRoutePreview = new ModalContext(9, []);
    static SimulatorControllerSelection = new ModalContext(10, []);
    static SimulatorRevisionStale = new ModalContext(11, []);
    static SimulatorNoHandoff = new ModalContext(12, []);
    static InputHelpPopup = new ModalContext(13, []);
}

export function ModalContext_$reflection() {
    return union_type("SIR.Client.ModalContext", [], ModalContext, () => [[], [["Item", EditorDomain_$reflection()]], [["Item", MapEditorTool_$reflection()]], [["Item", EditorGestureKind_$reflection()]], [], [], [], [], [], [], [], [], [], []]);
}

/**
 * Closed selectors make binding overlap validation deterministic and keep
 * catalog structure inspectable in both .NET and Fable.
 */
export class ModalContextSelector extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["AnyEditorContext", "AnySimulatorContext", "ExactContext"];
    }
    static AnyEditorContext = new ModalContextSelector(0, []);
    static AnySimulatorContext = new ModalContextSelector(1, []);
}

export function ModalContextSelector_$reflection() {
    return union_type("SIR.Client.ModalContextSelector", [], ModalContextSelector, () => [[], [], [["Item", ModalContext_$reflection()]]]);
}

export class ModalPrecedence extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["WorkspaceCommands", "ActiveTool", "ActiveGestureOrPreview", "HeldLayer", "TransientPopup", "InputPopup"];
    }
    static WorkspaceCommands = new ModalPrecedence(0, []);
    static ActiveTool = new ModalPrecedence(1, []);
    static ActiveGestureOrPreview = new ModalPrecedence(2, []);
    static HeldLayer = new ModalPrecedence(3, []);
    static TransientPopup = new ModalPrecedence(4, []);
    static InputPopup = new ModalPrecedence(5, []);
}

export function ModalPrecedence_$reflection() {
    return union_type("SIR.Client.ModalPrecedence", [], ModalPrecedence, () => [[], [], [], [], [], []]);
}

export class BindingAvailability extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Available", "Unavailable"];
    }
    static Available = new BindingAvailability(0, []);
}

export function BindingAvailability_$reflection() {
    return union_type("SIR.Client.BindingAvailability", [], BindingAvailability, () => [[], [["reason", string_type]]]);
}

export class SimulatorPanel extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ControllerPanel", "EventPanel", "SimulatorSamplePanel"];
    }
    static ControllerPanel = new SimulatorPanel(0, []);
    static EventPanel = new SimulatorPanel(1, []);
    static SimulatorSamplePanel = new SimulatorPanel(2, []);
}

export function SimulatorPanel_$reflection() {
    return union_type("SIR.Client.SimulatorPanel", [], SimulatorPanel, () => [[], [], []]);
}

/**
 * Commands describe application intent only. The web edge lowers them to its
 * Elmish message union; this module performs no browser or simulation I/O.
 */
export class ModalCommand extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["EditorCommand", "EditorWorkspaceCommand", "ChooseEditorDomain", "ToggleEditorCommandPanel", "ChooseSimulatorPanel", "ToggleSimulatorCommandPanel", "SimulatorCommand", "TraverseSimulatorUnit", "BeginSimulatorControllerSelection", "ChooseSimulatorController", "CommitSimulatorController", "CancelSimulatorController", "RequestSimulatorSandboxReset", "SetEditorPanHeld", "FocusUnitPresetSearch", "EditorDocumentCommand", "ToggleInputHelp"];
    }
    static ToggleEditorCommandPanel = new ModalCommand(3, []);
    static ToggleSimulatorCommandPanel = new ModalCommand(5, []);
    static BeginSimulatorControllerSelection = new ModalCommand(8, []);
    static CommitSimulatorController = new ModalCommand(10, []);
    static CancelSimulatorController = new ModalCommand(11, []);
    static RequestSimulatorSandboxReset = new ModalCommand(12, []);
    static FocusUnitPresetSearch = new ModalCommand(14, []);
    static ToggleInputHelp = new ModalCommand(16, []);
}

export function ModalCommand_$reflection() {
    return union_type("SIR.Client.ModalCommand", [], ModalCommand, () => [[["Item", MapEditorAction_$reflection()]], [["Item", EditorWorkspaceAction_$reflection()]], [["Item", EditorDomain_$reflection()]], [], [["Item", SimulatorPanel_$reflection()]], [], [["Item", SimulatorAction_$reflection()]], [["delta", int32_type]], [], [["Item", MapController_$reflection()]], [], [], [], [["Item", bool_type]], [], [["Item", EditorDocumentCommand_$reflection()]], []]);
}

export class HeldInput extends Union {
    constructor() {
        super();
        this.tag = 0;
        this.fields = [];
    }
    cases() {
        return ["EditorPan"];
    }
    static EditorPan = new HeldInput();
}

export function HeldInput_$reflection() {
    return union_type("SIR.Client.HeldInput", [], HeldInput, () => [[]]);
}

export class HeldInputSession extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["HeldInputSession"];
    }
}

export function HeldInputSession_$reflection() {
    return union_type("SIR.Client.HeldInputSession", [], HeldInputSession, () => [[["Item", class_type("Microsoft.FSharp.Collections.FSharpSet`1", [HeldInput_$reflection()])]]]);
}

export const HeldInputSessionModule_empty = new HeldInputSession(empty({
    Compare: (x, y) => (compare(x, y) | 0),
}));

export function HeldInputSessionModule_contains(input, _arg) {
    return contains(input, _arg.fields[0]);
}

export function HeldInputSessionModule_apply(command, _arg) {
    const inputs = _arg.fields[0];
    if (command.tag === 13) {
        if (command.fields[0]) {
            return new HeldInputSession(add(HeldInput.EditorPan, inputs));
        }
        else {
            return new HeldInputSession(remove(HeldInput.EditorPan, inputs));
        }
    }
    else {
        return new HeldInputSession(inputs);
    }
}

export function HeldInputSessionModule_recover(_arg) {
    return HeldInputSessionModule_empty;
}

export class ModalBinding$1 extends Record {
    constructor(Id, Context, Precedence, BindingGesture, Label, Group, Repeat, Availability, Command) {
        super();
        this.Id = Id;
        this.Context = Context;
        this.Precedence = Precedence;
        this.BindingGesture = BindingGesture;
        this.Label = Label;
        this.Group = Group;
        this.Repeat = Repeat;
        this.Availability = Availability;
        this.Command = Command;
    }
}

export function ModalBinding$1_$reflection(gen0) {
    return record_type("SIR.Client.ModalBinding`1", [gen0], ModalBinding$1, () => [["Id", string_type], ["Context", ModalContextSelector_$reflection()], ["Precedence", ModalPrecedence_$reflection()], ["BindingGesture", InputGesture_$reflection()], ["Label", string_type], ["Group", string_type], ["Repeat", RepeatPolicy_$reflection()], ["Availability", lambda_type(list_type(ModalContext_$reflection()), BindingAvailability_$reflection())], ["Command", gen0]]);
}

export class PossibleInput$1 extends Record {
    constructor(Id, InputGesture, Label, Group, Availability, Command) {
        super();
        this.Id = Id;
        this.InputGesture = InputGesture;
        this.Label = Label;
        this.Group = Group;
        this.Availability = Availability;
        this.Command = Command;
    }
}

export function PossibleInput$1_$reflection(gen0) {
    return record_type("SIR.Client.PossibleInput`1", [gen0], PossibleInput$1, () => [["Id", string_type], ["InputGesture", InputGesture_$reflection()], ["Label", string_type], ["Group", string_type], ["Availability", BindingAvailability_$reflection()], ["Command", gen0]]);
}

export class ModalProjection$1 extends Record {
    constructor(Contexts, Breadcrumb, Headline, Detail, PossibleInputs) {
        super();
        this.Contexts = Contexts;
        this.Breadcrumb = Breadcrumb;
        this.Headline = Headline;
        this.Detail = Detail;
        this.PossibleInputs = PossibleInputs;
    }
}

export function ModalProjection$1_$reflection(gen0) {
    return record_type("SIR.Client.ModalProjection`1", [gen0], ModalProjection$1, () => [["Contexts", list_type(ModalContext_$reflection())], ["Breadcrumb", list_type(string_type)], ["Headline", string_type], ["Detail", string_type], ["PossibleInputs", list_type(PossibleInput$1_$reflection(gen0))]]);
}

export class InputResolution$1 extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Resolved", "NoMatch", "NoAvailableMatch"];
    }
    static NoMatch = new InputResolution$1(1, []);
}

export function InputResolution$1_$reflection(gen0) {
    return union_type("SIR.Client.InputResolution`1", [gen0], InputResolution$1, () => [[["Item", PossibleInput$1_$reflection(gen0)]], [], [["Item", list_type(PossibleInput$1_$reflection(gen0))]]]);
}

export class CatalogDiagnostic extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["DuplicateBindingId", "EqualPrecedenceGestureConflict"];
    }
}

export function CatalogDiagnostic_$reflection() {
    return union_type("SIR.Client.CatalogDiagnostic", [], CatalogDiagnostic, () => [[["id", string_type]], [["firstId", string_type], ["secondId", string_type], ["precedence", ModalPrecedence_$reflection()], ["gesture", InputGesture_$reflection()]]]);
}

export class EditorModalFacts extends Record {
    constructor(Editor, ActiveDomain, PanHeld, InputHelpExpanded) {
        super();
        this.Editor = Editor;
        this.ActiveDomain = ActiveDomain;
        this.PanHeld = PanHeld;
        this.InputHelpExpanded = InputHelpExpanded;
    }
}

export function EditorModalFacts_$reflection() {
    return record_type("SIR.Client.EditorModalFacts", [], EditorModalFacts, () => [["Editor", MapEditorState_$reflection()], ["ActiveDomain", EditorDomain_$reflection()], ["PanHeld", bool_type], ["InputHelpExpanded", bool_type]]);
}

export class SimulatorModalFacts extends Record {
    constructor(SimulatorHandoffPresent, SimulatorIsRunning, SimulatorHasRoutePreview, SimulatorControllerSelection, SimulatorRevisionIsStale, InputHelpExpanded) {
        super();
        this.SimulatorHandoffPresent = SimulatorHandoffPresent;
        this.SimulatorIsRunning = SimulatorIsRunning;
        this.SimulatorHasRoutePreview = SimulatorHasRoutePreview;
        this.SimulatorControllerSelection = SimulatorControllerSelection;
        this.SimulatorRevisionIsStale = SimulatorRevisionIsStale;
        this.InputHelpExpanded = InputHelpExpanded;
    }
}

export function SimulatorModalFacts_$reflection() {
    return record_type("SIR.Client.SimulatorModalFacts", [], SimulatorModalFacts, () => [["SimulatorHandoffPresent", bool_type], ["SimulatorIsRunning", bool_type], ["SimulatorHasRoutePreview", bool_type], ["SimulatorControllerSelection", option_type(MapController_$reflection())], ["SimulatorRevisionIsStale", bool_type], ["InputHelpExpanded", bool_type]]);
}

export function ModalInput_acceptsTarget(target) {
    switch (target.tag) {
        case 4:
            return true;
        default:
            return false;
    }
}

export function ModalInput_precedenceRank(precedence) {
    switch (precedence.tag) {
        case 1:
            return 1;
        case 2:
            return 2;
        case 3:
            return 3;
        case 4:
            return 4;
        case 5:
            return 5;
        default:
            return 0;
    }
}

function ModalInput_isEditorContext(_arg) {
    switch (_arg.tag) {
        case 0:
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
            return true;
        default:
            return false;
    }
}

function ModalInput_isSimulatorContext(_arg) {
    switch (_arg.tag) {
        case 6:
        case 7:
        case 8:
        case 9:
        case 10:
        case 11:
        case 12:
            return true;
        default:
            return false;
    }
}

function ModalInput_isSharedPopupContext(_arg) {
    if (_arg.tag === 13) {
        return true;
    }
    else {
        return false;
    }
}

function ModalInput_editorContextCategory(_arg) {
    switch (_arg.tag) {
        case 0:
            return 0;
        case 1:
            return 1;
        case 2:
            return 2;
        case 3:
            return 3;
        case 4:
            return 4;
        case 5:
            return 5;
        default:
            return -1;
    }
}

function ModalInput_simulatorContextsCanCoexist(left, right) {
    let matchResult;
    switch (left.tag) {
        case 7: {
            switch (right.tag) {
                case 8: {
                    matchResult = 0;
                    break;
                }
                case 12: {
                    matchResult = 2;
                    break;
                }
                default:
                    matchResult = 3;
            }
            break;
        }
        case 8: {
            switch (right.tag) {
                case 7: {
                    matchResult = 0;
                    break;
                }
                case 12: {
                    matchResult = 2;
                    break;
                }
                default:
                    matchResult = 3;
            }
            break;
        }
        case 9: {
            switch (right.tag) {
                case 10: {
                    matchResult = 1;
                    break;
                }
                case 12: {
                    matchResult = 2;
                    break;
                }
                default:
                    matchResult = 3;
            }
            break;
        }
        case 10: {
            switch (right.tag) {
                case 9: {
                    matchResult = 1;
                    break;
                }
                case 12: {
                    matchResult = 2;
                    break;
                }
                default:
                    matchResult = 3;
            }
            break;
        }
        case 12: {
            switch (right.tag) {
                case 7:
                case 8:
                case 9:
                case 10:
                case 11: {
                    matchResult = 2;
                    break;
                }
                default:
                    matchResult = 3;
            }
            break;
        }
        case 11: {
            if (right.tag === 12) {
                matchResult = 2;
            }
            else {
                matchResult = 3;
            }
            break;
        }
        default:
            matchResult = 3;
    }
    switch (matchResult) {
        case 0:
            return false;
        case 1:
            return false;
        case 2:
            return false;
        default:
            return true;
    }
}

function ModalInput_exactContextsOverlap(left, right) {
    if ((equals(left, right) ? true : ModalInput_isSharedPopupContext(left)) ? true : ModalInput_isSharedPopupContext(right)) {
        return true;
    }
    else if (ModalInput_isEditorContext(left) && ModalInput_isEditorContext(right)) {
        return ModalInput_editorContextCategory(left) !== ModalInput_editorContextCategory(right);
    }
    else if (ModalInput_isSimulatorContext(left) && ModalInput_isSimulatorContext(right)) {
        return ModalInput_simulatorContextsCanCoexist(left, right);
    }
    else {
        return false;
    }
}

function ModalInput_selectorMatches(contexts, _arg) {
    switch (_arg.tag) {
        case 1:
            return exists(ModalInput_isSimulatorContext, contexts);
        case 2:
            return contains_1(_arg.fields[0], contexts, {
                Equals: equals,
                GetHashCode: (x) => (safeHash(x) | 0),
            });
        default:
            return exists(ModalInput_isEditorContext, contexts);
    }
}

export function ModalInput_selectorsOverlap(left, right) {
    let matchResult, context, context_1, leftContext, rightContext;
    switch (left.tag) {
        case 1: {
            switch (right.tag) {
                case 2: {
                    matchResult = 2;
                    context_1 = right.fields[0];
                    break;
                }
                case 0: {
                    matchResult = 4;
                    break;
                }
                default:
                    matchResult = 0;
            }
            break;
        }
        case 2: {
            switch (right.tag) {
                case 1: {
                    matchResult = 2;
                    context_1 = left.fields[0];
                    break;
                }
                case 2: {
                    matchResult = 3;
                    leftContext = left.fields[0];
                    rightContext = right.fields[0];
                    break;
                }
                default: {
                    matchResult = 1;
                    context = left.fields[0];
                }
            }
            break;
        }
        default:
            switch (right.tag) {
                case 2: {
                    matchResult = 1;
                    context = right.fields[0];
                    break;
                }
                case 1: {
                    matchResult = 4;
                    break;
                }
                default:
                    matchResult = 0;
            }
    }
    switch (matchResult) {
        case 0:
            return true;
        case 1:
            if (ModalInput_isEditorContext(context)) {
                return true;
            }
            else {
                return ModalInput_isSharedPopupContext(context);
            }
        case 2:
            if (ModalInput_isSimulatorContext(context_1)) {
                return true;
            }
            else {
                return ModalInput_isSharedPopupContext(context_1);
            }
        case 3:
            return ModalInput_exactContextsOverlap(leftContext, rightContext);
        default:
            return false;
    }
}

function ModalInput_sameGesture(left, right) {
    if (NormalizedKeyModule_sameProducedKey(left.Key, right.Key) && equals(left.Modifiers, right.Modifiers)) {
        return equals(left.Phase, right.Phase);
    }
    else {
        return false;
    }
}

function ModalInput_gestureMatches(actual, expected) {
    return ModalInput_sameGesture(actual, expected);
}

function ModalInput_toPossible(contexts, binding) {
    return new PossibleInput$1(binding.Id, binding.BindingGesture, binding.Label, binding.Group, binding.Availability(contexts), binding.Command);
}

export function ModalInput_deriveEditorContexts(facts) {
    let gestureContexts;
    const matchValue = facts.Editor.Gesture;
    gestureContexts = ((matchValue.tag === 1) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.SelectedObjectActions])) : ((matchValue.tag === 2) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.BoxSelection])) : ((matchValue.tag === 3) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.CommandPreview])) : ((matchValue.tag === 4) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.UnitMovePreview])) : ((matchValue.tag === 5) ? singleton(new ModalContext(/* EditorGesture */ 3, [new EditorGestureKind(/* TerrainPreview */ 4, [matchValue.fields[0]])])) : ((matchValue.tag === 6) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.EdgePolyline])) : empty_1()))))));
    let regionContexts;
    const matchValue_1 = facts.Editor.RegionKeyboardMode;
    regionContexts = ((matchValue_1.tag === 1) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionPurpose])) : ((matchValue_1.tag === 2) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionShape])) : ((matchValue_1.tag === 3) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionRectangleMode])) : ((matchValue_1.tag === 4) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionPolygonMode])) : ((matchValue_1.tag === 5) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionMove])) : ((matchValue_1.tag === 6) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionResize])) : ((matchValue_1.tag === 7) ? singleton(new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionVertex])) : empty_1())))))));
    return toList(delay(() => append(singleton_1(ModalContext.EditorBase), delay(() => append(singleton_1(new ModalContext(/* EditorDomain */ 1, [facts.ActiveDomain])), delay(() => append(singleton_1(new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])), delay(() => append(gestureContexts, delay(() => append(regionContexts, delay(() => append(facts.PanHeld ? singleton_1(ModalContext.EditorPanHeld) : empty_2(), delay(() => append((facts.Editor.PendingDestructiveChange != null) ? singleton_1(ModalContext.EditorDestructiveConfirmation) : empty_2(), delay(() => (facts.InputHelpExpanded ? singleton_1(ModalContext.InputHelpPopup) : empty_2())))))))))))))))));
}

export function ModalInput_deriveSimulatorContexts(facts) {
    if (!facts.SimulatorHandoffPresent) {
        return toList(delay(() => append(singleton_1(ModalContext.SimulatorBase), delay(() => append(singleton_1(ModalContext.SimulatorNoHandoff), delay(() => (facts.InputHelpExpanded ? singleton_1(ModalContext.InputHelpPopup) : empty_2())))))));
    }
    else {
        return toList(delay(() => append(singleton_1(ModalContext.SimulatorBase), delay(() => append(singleton_1(facts.SimulatorIsRunning ? ModalContext.SimulatorRunning : ModalContext.SimulatorPaused), delay(() => append(facts.SimulatorHasRoutePreview ? singleton_1(ModalContext.SimulatorRoutePreview) : empty_2(), delay(() => append((facts.SimulatorControllerSelection != null) ? singleton_1(ModalContext.SimulatorControllerSelection) : empty_2(), delay(() => append(facts.SimulatorRevisionIsStale ? singleton_1(ModalContext.SimulatorRevisionStale) : empty_2(), delay(() => (facts.InputHelpExpanded ? singleton_1(ModalContext.InputHelpPopup) : empty_2())))))))))))));
    }
}

/**
 * Selects the available binding at the highest precedence. Catalog order
 * never decides a tie: stable IDs provide deterministic behavior even
 * while an invalid catalog is being diagnosed.
 */
export function ModalInput_resolve(contexts, gesture, isRepeat, catalog) {
    const matching = sortBy((binding_1) => [op_UnaryNegation_Int32(ModalInput_precedenceRank(binding_1.Precedence)), binding_1.Id], filter((binding) => {
        if (ModalInput_selectorMatches(contexts, binding.Context) && ModalInput_gestureMatches(gesture, binding.BindingGesture)) {
            if (!contains_1(ModalContext.EditorPanHeld, contexts, {
                Equals: equals,
                GetHashCode: (x) => (safeHash(x) | 0),
            })) {
                return true;
            }
            else {
                return ModalInput_precedenceRank(binding.Precedence) >= ModalInput_precedenceRank(ModalPrecedence.HeldLayer);
            }
        }
        else {
            return false;
        }
    }, catalog), {
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    });
    if (!isEmpty(matching)) {
        const highestRank = ModalInput_precedenceRank(head(matching).Precedence) | 0;
        const owningBindings = takeWhile((binding_2) => (ModalInput_precedenceRank(binding_2.Precedence) === highestRank), matching);
        if (isRepeat && forAll((binding_3) => equals(binding_3.Repeat, RepeatPolicy.IgnoreRepeat), owningBindings)) {
            return InputResolution$1.NoMatch;
        }
        else {
            const possible = map((binding_5) => ModalInput_toPossible(contexts, binding_5), filter((binding_4) => {
                if (!isRepeat) {
                    return true;
                }
                else {
                    return equals(binding_4.Repeat, RepeatPolicy.AllowRepeat);
                }
            }, owningBindings));
            const matchValue = tryFind((input) => equals(input.Availability, BindingAvailability.Available), possible);
            if (matchValue == null) {
                return new InputResolution$1(/* NoAvailableMatch */ 2, [possible]);
            }
            else {
                return new InputResolution$1(/* Resolved */ 0, [matchValue]);
            }
        }
    }
    else {
        return InputResolution$1.NoMatch;
    }
}

export function ModalInput_possibleInputs(contexts, catalog) {
    return sortBy((input_2) => [input_2.Group, input_2.Label, input_2.Id], filter((input_1) => {
        const matchValue = ModalInput_resolve(contexts, input_1.InputGesture, false, catalog);
        switch (matchValue.tag) {
            case 1:
            case 2:
                return false;
            default:
                return matchValue.fields[0].Id === input_1.Id;
        }
    }, filter((input) => equals(input.Availability, BindingAvailability.Available), map((binding_1) => ModalInput_toPossible(contexts, binding_1), filter((binding) => ModalInput_selectorMatches(contexts, binding.Context), catalog)))), {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    });
}

export function ModalInput_tryAvailableCommandById(contexts, id, catalog) {
    const option_1 = tryFind((binding) => {
        if ((binding.Id === id) && ModalInput_selectorMatches(contexts, binding.Context)) {
            return equals(binding.Availability(contexts), BindingAvailability.Available);
        }
        else {
            return false;
        }
    }, catalog);
    if (option_1 != null) {
        return some(option_1.Command);
    }
    else {
        return undefined;
    }
}

/**
 * Stable binding identifiers accepted in persisted tactical profiles.
 * This is deliberately stricter than an editor./simulator. prefix check:
 * a typo must not become a dormant override that silently activates later.
 */
export function ModalInput_isKnownCommandId(id) {
    const suffixIn = (prefix, values) => {
        if (startsWith(id, prefix, 4)) {
            return contains(substring(id, prefix.length), ofList(values, {
                Compare: (x, y) => (comparePrimitives(x, y) | 0),
            }));
        }
        else {
            return false;
        }
    };
    if (((((((((((((((((suffixIn("editor.camera.", ofArray(["fit", "frame-selection", "pan-cancel", "pan-east", "pan-east-large", "pan-held", "pan-north", "pan-north-large", "pan-release", "pan-south", "pan-south-large", "pan-west", "pan-west-large", "reset"])) ? true : suffixIn("editor.confirmation.", ofArray(["cancel", "confirm"]))) ? true : suffixIn("editor.cursor.", ofArray(["east", "north", "south", "west", "next-object", "previous-object"]))) ? true : suffixIn("editor.document.", ofArray(["background", "bundle", "clear", "exit", "export", "import", "layers", "new", "resize", "views"]))) ? true : suffixIn("editor.domain.", ofArray(["document", "edges", "terrain", "units", "zones"]))) ? true : suffixIn("editor.edge.", ofArray(["activate", "cursor.east", "cursor.north", "cursor.south", "cursor.west", "door.toggle", "erase", "join", "kind.door", "kind.wall", "kind.window", "orientation.rotate", "polyline.backtrack", "polyline.east", "polyline.north", "polyline.south", "polyline.west", "split"]))) ? true : suffixIn("editor.gesture.", ofArray(["cancel", "commit"]))) ? true : suffixIn("editor.help.", ofArray(["close", "toggle"]))) ? true : suffixIn("editor.history.", ofArray(["redo-shift-z", "redo-y", "undo"]))) ? true : (id === "editor.inspector.toggle")) ? true : (id === "editor.mode.select")) ? true : (id === "editor.panel.toggle")) ? true : suffixIn("editor.selection.", ofArray(["actions.copy", "actions.delete", "actions.duplicate", "actions.inspector", "actions.move", "all", "all-domain", "box.add", "box.begin", "box.east", "box.east-extended", "box.north", "box.north-extended", "box.south", "box.south-extended", "box.west", "box.west-extended", "clear", "copy", "delete", "delete-backspace", "duplicate", "paste", "single", "toggle"]))) ? true : suffixIn("editor.terrain.", ofArray(["activate", "brush.decrease", "brush.increase", "cursor.east", "cursor.north", "cursor.paint-east", "cursor.paint-north", "cursor.paint-south", "cursor.paint-west", "cursor.south", "cursor.west", "exit", "gesture.east", "gesture.east-extended", "gesture.north", "gesture.north-extended", "gesture.reset", "gesture.south", "gesture.south-extended", "gesture.west", "gesture.west-extended", "value.open", "value.rough", "value.blocked", "value.objective"]))) ? true : suffixIn("editor.tool.terrain.", ofArray(["erase", "eyedropper", "flood-fill", "line", "pencil", "rectangle"]))) ? true : suffixIn("editor.unit.", ofArray(["move.begin", "move.east", "move.east-large", "move.north", "move.north-large", "move.reset", "move.south", "move.south-large", "move.west", "move.west-large", "place.browse", "place.cancel", "place.commit", "place.commit-return", "place.east", "place.next-preset", "place.north", "place.previous-preset", "place.south", "place.west", "preset.arm", "preset.exit", "preset.first", "preset.last", "preset.next-arrow", "preset.next-bracket", "preset.next-faction", "preset.previous-arrow", "preset.previous-bracket", "preset.previous-faction", "preset.search"]))) ? true : suffixIn("editor.validation.", ofArray(["next", "previous"]))) ? true : suffixIn("editor.region.", ofArray(["create.begin", "cursor.east", "cursor.north", "cursor.south", "cursor.west", "delete", "edit.move", "edit.purpose", "edit.resize", "edit.vertices", "exit", "move.east", "move.east-large", "move.north", "move.north-large", "move.south", "move.south-large", "move.west", "move.west-large", "move.cancel", "move.commit", "move.reset", "polygon.east", "polygon.north", "polygon.south", "polygon.west", "polygon.backtrack", "polygon.cancel", "polygon.commit", "polygon.vertex", "purpose.objective", "purpose.blue", "purpose.red", "purpose.cancel", "purpose.commit", "rectangle.east", "rectangle.north", "rectangle.south", "rectangle.west", "rectangle.activate", "rectangle.cancel", "rectangle.reset", "resize.cancel", "resize.commit", "resize.height.decrease", "resize.height.increase", "resize.origin.east", "resize.origin.north", "resize.origin.south", "resize.origin.west", "resize.reset", "resize.width.decrease", "resize.width.increase", "select", "shape.back", "shape.polygon", "shape.rectangle", "vertex.east", "vertex.east-large", "vertex.north", "vertex.north-large", "vertex.south", "vertex.south-large", "vertex.west", "vertex.west-large", "vertex.cancel", "vertex.commit", "vertex.next", "vertex.previous", "vertex.reset"]))) {
        return true;
    }
    else {
        return suffixIn("simulator.", ofArray(["controller.begin", "controller.cancel", "controller.commit", "controller.general", "controller.manual", "controller.scripted", "help.toggle", "panel.controls", "panel.events", "panel.samples", "panel.toggle", "preview.cancel", "preview.commit", "preview.east", "preview.fast-east", "preview.fast-north", "preview.fast-south", "preview.fast-west", "preview.north", "preview.reset", "preview.south", "preview.west", "reset.request", "run.toggle-k", "run.toggle-space", "step", "unit.next", "unit.previous"]));
    }
}

function ModalInput_key(value, modifiers, phase) {
    return new InputGesture(NormalizedKeyModule_create(value, undefined), modifiers, phase);
}

const ModalInput_plain = KeyModifiersModule_none;

function ModalInput_control(shift) {
    return new KeyModifiers(true, shift, KeyModifiersModule_none.Alt);
}

function ModalInput_available(_arg) {
    return BindingAvailability.Available;
}

function ModalInput_binding(id, context, precedence, gesture, label, group, repeat, availability, command) {
    return new ModalBinding$1(id, context, precedence, gesture, label, group, repeat, availability, command);
}

export function ModalInput_editorCatalog(facts) {
    const selectionAvailable = (_arg) => {
        if (FSharpSet__get_IsEmpty(facts.Editor.SelectedUnits) && (facts.Editor.SelectedRegion == null)) {
            return new BindingAvailability(/* Unavailable */ 1, ["Nothing is selected."]);
        }
        else {
            return BindingAvailability.Available;
        }
    };
    const unitSelectionAvailable = (_arg_1) => {
        if (FSharpSet__get_IsEmpty(facts.Editor.SelectedUnits)) {
            return new BindingAvailability(/* Unavailable */ 1, ["No units are selected."]);
        }
        else {
            return BindingAvailability.Available;
        }
    };
    const historyAvailable = (history, reason, _arg_2) => {
        if (isEmpty(history)) {
            return new BindingAvailability(/* Unavailable */ 1, [reason]);
        }
        else {
            return BindingAvailability.Available;
        }
    };
    const selectableDomainAvailable = (_arg_4) => {
        if (FSharpMap__get_IsEmpty(facts.Editor.Map.Units)) {
            return new BindingAvailability(/* Unavailable */ 1, ["The active domain contains no selectable objects."]);
        }
        else {
            return BindingAvailability.Available;
        }
    };
    const validationAvailable = (_arg_5) => {
        if (facts.Editor.Issues.length === 0) {
            return new BindingAvailability(/* Unavailable */ 1, ["The map has no validation issues."]);
        }
        else {
            return BindingAvailability.Available;
        }
    };
    const selectedRegionAvailable = (_arg_6) => {
        if (facts.Editor.SelectedRegion != null) {
            return BindingAvailability.Available;
        }
        else {
            return new BindingAvailability(/* Unavailable */ 1, ["Select a region first."]);
        }
    };
    const editorKey = (id_2, value, modifiers, label, group, repeat, availability, command) => ModalInput_binding(id_2, ModalContextSelector.AnyEditorContext, ModalPrecedence.WorkspaceCommands, ModalInput_key(value, modifiers, InputPhase.KeyDown), label, group, repeat, availability, command);
    return toList(delay(() => append(singleton_1(editorKey("editor.help.toggle", "?", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), "Show or hide possible inputs", "Help", RepeatPolicy.IgnoreRepeat, ModalInput_available, ModalCommand.ToggleInputHelp)), delay(() => append(singleton_1(editorKey("editor.panel.toggle", "F2", ModalInput_plain, "Show or hide the active command panel", "Panels", RepeatPolicy.IgnoreRepeat, ModalInput_available, ModalCommand.ToggleEditorCommandPanel)), delay(() => append(singleton_1(editorKey("editor.inspector.toggle", "F3", ModalInput_plain, "Show or hide the selected-object inspector", "Panels", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorWorkspaceCommand */ 1, [EditorWorkspaceAction.ToggleEditorInspector]))), delay(() => append(singleton_1(editorKey("editor.history.undo", "z", ModalInput_control(false), "Undo", "Edit", RepeatPolicy.IgnoreRepeat, curry3(historyAvailable)(facts.Editor.UndoHistory)("There is nothing to undo."), new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.UndoEditorCommand]))), delay(() => append(singleton_1(editorKey("editor.history.redo-shift-z", "z", ModalInput_control(true), "Redo", "Edit", RepeatPolicy.IgnoreRepeat, curry3(historyAvailable)(facts.Editor.RedoHistory)("There is nothing to redo."), new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.RedoEditorCommand]))), delay(() => append(singleton_1(editorKey("editor.history.redo-y", "y", ModalInput_control(false), "Redo", "Edit", RepeatPolicy.IgnoreRepeat, curry3(historyAvailable)(facts.Editor.RedoHistory)("There is nothing to redo."), new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.RedoEditorCommand]))), delay(() => append(singleton_1(editorKey("editor.selection.copy", "c", ModalInput_control(false), "Copy selected units", "Edit", RepeatPolicy.IgnoreRepeat, unitSelectionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CopyEditorSelection]))), delay(() => append(singleton_1(editorKey("editor.selection.paste", "v", ModalInput_control(false), "Paste the editor clipboard", "Edit", RepeatPolicy.IgnoreRepeat, (_arg_3) => {
        if (facts.Editor.Clipboard != null) {
            return BindingAvailability.Available;
        }
        else {
            return new BindingAvailability(/* Unavailable */ 1, ["The editor clipboard is empty."]);
        }
    }, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.PasteEditorClipboard]))), delay(() => append(singleton_1(editorKey("editor.selection.duplicate", "d", ModalInput_control(false), "Duplicate selected units", "Edit", RepeatPolicy.IgnoreRepeat, unitSelectionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.DuplicateEditorSelection]))), delay(() => append(singleton_1(editorKey("editor.selection.all", "a", ModalInput_control(false), "Select all in the active domain", "Selection", RepeatPolicy.IgnoreRepeat, selectableDomainAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.SelectAllInActiveDomain]))), delay(() => append(singleton_1(editorKey("editor.selection.delete", "Delete", ModalInput_plain, "Delete the selection", "Selection", RepeatPolicy.IgnoreRepeat, selectionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.DeleteEditorSelection]))), delay(() => append(singleton_1(editorKey("editor.selection.delete-backspace", "Backspace", ModalInput_plain, "Delete the selection", "Selection", RepeatPolicy.IgnoreRepeat, selectionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.DeleteEditorSelection]))), delay(() => append(singleton_1(editorKey("editor.camera.fit", "0", ModalInput_plain, "Fit the complete map", "View", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorWorkspaceCommand */ 1, [EditorWorkspaceAction.FitEditorBoard]))), delay(() => append(singleton_1(editorKey("editor.camera.reset", "1", ModalInput_plain, "Reset the camera to 100%", "View", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorWorkspaceCommand */ 1, [EditorWorkspaceAction.ResetEditorCamera]))), delay(() => append(singleton_1(editorKey("editor.camera.frame-selection", "f", ModalInput_plain, "Frame the selection", "View", RepeatPolicy.IgnoreRepeat, selectionAvailable, new ModalCommand(/* EditorWorkspaceCommand */ 1, [EditorWorkspaceAction.FrameEditorSelection]))), delay(() => append(singleton_1(editorKey("editor.mode.select", "v", ModalInput_plain, "Enter Select", "Modes", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [MapEditorTool.Select])]))), delay(() => append(singleton_1(editorKey("editor.domain.terrain", "t", ModalInput_plain, "Open Terrain commands", "Modes", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* ChooseEditorDomain */ 2, [EditorDomain.TerrainDomain]))), delay(() => append(singleton_1(editorKey("editor.domain.units", "u", ModalInput_plain, "Open Unit commands", "Modes", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* ChooseEditorDomain */ 2, [EditorDomain.UnitDomain]))), delay(() => append(singleton_1(editorKey("editor.domain.edges", "e", ModalInput_plain, "Open Edge commands", "Modes", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* ChooseEditorDomain */ 2, [EditorDomain.EdgeDomain]))), delay(() => append(singleton_1(editorKey("editor.domain.zones", "z", ModalInput_plain, "Open Zone commands", "Modes", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* ChooseEditorDomain */ 2, [EditorDomain.RegionDomain]))), delay(() => append(singleton_1(editorKey("editor.domain.document", "m", ModalInput_plain, "Open Document commands", "Modes", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* ChooseEditorDomain */ 2, [EditorDomain.DocumentDomain]))), delay(() => append(singleton_1(editorKey("editor.tool.terrain.pencil", "p", ModalInput_plain, "Choose Pencil", "Terrain tools", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [new MapEditorTool(/* Terrain */ 2, [TerrainAuthoringTool.PencilTool])])]))), delay(() => append(singleton_1(editorKey("editor.tool.terrain.rectangle", "r", ModalInput_plain, "Choose Rectangle", "Terrain tools", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [new MapEditorTool(/* Terrain */ 2, [TerrainAuthoringTool.RectangleTool])])]))), delay(() => append(singleton_1(editorKey("editor.tool.terrain.line", "l", ModalInput_plain, "Choose Line", "Terrain tools", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [new MapEditorTool(/* Terrain */ 2, [TerrainAuthoringTool.LineTool])])]))), delay(() => append(singleton_1(editorKey("editor.tool.terrain.flood-fill", "g", ModalInput_plain, "Choose Flood fill", "Terrain tools", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [new MapEditorTool(/* Terrain */ 2, [TerrainAuthoringTool.FloodFillTool])])]))), delay(() => append(singleton_1(editorKey("editor.tool.terrain.eyedropper", "i", ModalInput_plain, "Choose Eyedropper", "Terrain tools", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [new MapEditorTool(/* Terrain */ 2, [TerrainAuthoringTool.EyedropperTool])])]))), delay(() => append(singleton_1(editorKey("editor.tool.terrain.erase", "x", ModalInput_plain, "Choose Eraser", "Terrain tools", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [new MapEditorTool(/* Terrain */ 2, [TerrainAuthoringTool.EraseTool])])]))), delay(() => append(singleton_1(editorKey("editor.validation.previous", "[", ModalInput_plain, "Select the previous validation issue", "Validation", RepeatPolicy.AllowRepeat, validationAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.SelectPreviousIssue]))), delay(() => append(singleton_1(editorKey("editor.validation.next", "]", ModalInput_plain, "Select the next validation issue", "Validation", RepeatPolicy.AllowRepeat, validationAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.SelectNextIssue]))), delay(() => append(singleton_1(ModalInput_binding("editor.camera.pan-held", ModalContextSelector.AnyEditorContext, ModalPrecedence.HeldLayer, ModalInput_key("Space", ModalInput_plain, InputPhase.KeyDown), "Hold to pan the map", "View", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* SetEditorPanHeld */ 13, [true]))), delay(() => append(singleton_1(ModalInput_binding("editor.camera.pan-release", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.EditorPanHeld]), ModalPrecedence.HeldLayer, ModalInput_key("Space", ModalInput_plain, InputPhase.KeyUp), "Release held pan", "View", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* SetEditorPanHeld */ 13, [false]))), delay(() => {
        let pan;
        return append(facts.PanHeld ? ((pan = ((id_3, value_1, modifiers_1, x, y) => ModalInput_binding(id_3, new ModalContextSelector(/* ExactContext */ 2, [ModalContext.EditorPanHeld]), ModalPrecedence.HeldLayer, ModalInput_key(value_1, modifiers_1, InputPhase.KeyDown), "Pan the map", "View", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorWorkspaceCommand */ 1, [new EditorWorkspaceAction(/* PanEditorBy */ 1, [x, y])]))), append(singleton_1(pan("editor.camera.pan-west", "ArrowLeft", ModalInput_plain, 40, 0)), delay(() => append(singleton_1(pan("editor.camera.pan-east", "ArrowRight", ModalInput_plain, -40, 0)), delay(() => append(singleton_1(pan("editor.camera.pan-north", "ArrowUp", ModalInput_plain, 0, 40)), delay(() => append(singleton_1(pan("editor.camera.pan-south", "ArrowDown", ModalInput_plain, 0, -40)), delay(() => append(singleton_1(pan("editor.camera.pan-west-large", "ArrowLeft", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 120, 0)), delay(() => append(singleton_1(pan("editor.camera.pan-east-large", "ArrowRight", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), -120, 0)), delay(() => append(singleton_1(pan("editor.camera.pan-north-large", "ArrowUp", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 0, 120)), delay(() => append(singleton_1(pan("editor.camera.pan-south-large", "ArrowDown", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 0, -120)), delay(() => singleton_1(ModalInput_binding("editor.camera.pan-cancel", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.EditorPanHeld]), ModalPrecedence.HeldLayer, ModalInput_key("Escape", ModalInput_plain, InputPhase.KeyDown), "Release held pan", "View", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* SetEditorPanHeld */ 13, [false]))))))))))))))))))))) : empty_2(), delay(() => append(facts.InputHelpExpanded ? singleton_1(ModalInput_binding("editor.help.close", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.InputHelpPopup]), ModalPrecedence.InputPopup, ModalInput_key("Escape", ModalInput_plain, InputPhase.KeyDown), "Close possible inputs", "Help", RepeatPolicy.IgnoreRepeat, ModalInput_available, ModalCommand.ToggleInputHelp)) : ((facts.Editor.PendingDestructiveChange != null) ? append(singleton_1(ModalInput_binding("editor.confirmation.confirm", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.EditorDestructiveConfirmation]), ModalPrecedence.TransientPopup, ModalInput_key("Enter", ModalInput_plain, InputPhase.KeyDown), "Confirm the pending destructive change", "Current operation", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ConfirmDestructiveChange]))), delay(() => singleton_1(ModalInput_binding("editor.confirmation.cancel", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.EditorDestructiveConfirmation]), ModalPrecedence.TransientPopup, ModalInput_key("Escape", ModalInput_plain, InputPhase.KeyDown), "Cancel the pending destructive change", "Current operation", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelDestructiveChange]))))) : (!equals(facts.Editor.Gesture, EditorGesture.IdleGesture) ? singleton_1(ModalInput_binding("editor.gesture.cancel", ModalContextSelector.AnyEditorContext, ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Escape", ModalInput_plain, InputPhase.KeyDown), "Cancel the current operation", "Current operation", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelEditorGesture]))) : singleton_1(ModalInput_binding("editor.selection.clear", ModalContextSelector.AnyEditorContext, ModalPrecedence.WorkspaceCommands, ModalInput_key("Escape", ModalInput_plain, InputPhase.KeyDown), "Clear the selection", "Selection", RepeatPolicy.IgnoreRepeat, selectionAvailable, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* SelectEditorUnit */ 77, [undefined])]))))), delay(() => {
            let matchValue_2;
            return append((matchValue_2 = facts.Editor.Gesture, (matchValue_2.tag === 0) ? (empty_2()) : ((matchValue_2.tag === 1) ? (empty_2()) : singleton_1(ModalInput_binding("editor.gesture.commit", ModalContextSelector.AnyEditorContext, ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Enter", ModalInput_plain, InputPhase.KeyDown), "Commit the current operation", "Current operation", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CommitEditorGesture]))))), delay(() => {
                let matchValue_3, current, anchor, clamp, move, extend, current_1, extend_1;
                return append((matchValue_3 = facts.Editor.Gesture, (matchValue_3.tag === 1) ? append(singleton_1(ModalInput_binding("editor.selection.actions.copy", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.SelectedObjectActions])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("c", ModalInput_plain, InputPhase.KeyDown), "Copy selected units", "Selected-object actions", RepeatPolicy.IgnoreRepeat, unitSelectionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CopyEditorSelection]))), delay(() => append(singleton_1(ModalInput_binding("editor.selection.actions.duplicate", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.SelectedObjectActions])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("d", ModalInput_plain, InputPhase.KeyDown), "Duplicate selected units", "Selected-object actions", RepeatPolicy.IgnoreRepeat, unitSelectionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.DuplicateEditorSelection]))), delay(() => append(singleton_1(ModalInput_binding("editor.selection.actions.delete", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.SelectedObjectActions])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Delete", ModalInput_plain, InputPhase.KeyDown), "Delete the selection", "Selected-object actions", RepeatPolicy.IgnoreRepeat, selectionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.DeleteEditorSelection]))), delay(() => append(singleton_1(ModalInput_binding("editor.selection.actions.inspector", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.SelectedObjectActions])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("i", ModalInput_plain, InputPhase.KeyDown), "Open selected-object inspector", "Selected-object actions", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorWorkspaceCommand */ 1, [EditorWorkspaceAction.ToggleEditorInspector]))), delay(() => singleton_1(ModalInput_binding("editor.selection.actions.move", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.SelectedObjectActions])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("m", ModalInput_plain, InputPhase.KeyDown), "Move selected units", "Selected-object actions", RepeatPolicy.IgnoreRepeat, unitSelectionAvailable, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* BeginUnitMove */ 57, [facts.Editor.KeyboardCursor.Cell])]))))))))))) : ((matchValue_3.tag === 2) ? ((current = matchValue_3.fields[1], (anchor = matchValue_3.fields[0], (clamp = ((minimum, maximum, value_2) => max((x_2, y_2) => (compare(x_2, y_2) | 0), minimum, min((x_1, y_1) => (compare(x_1, y_1) | 0), maximum, value_2))), (move = ((id_4, value_3, modifiers_2, dx, dy) => ModalInput_binding(id_4, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.BoxSelection])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key(value_3, modifiers_2, InputPhase.KeyDown), "Move the box corner", "Selection", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ExtendEditorBoxSelection */ 81, [new EditorCellAddress(clamp(0, facts.Editor.Map.Width - 1, current.CellColumn + dx), clamp(0, facts.Editor.Map.Height - 1, current.CellRow + dy))])]))), append(collect((matchValue_4) => {
                    const suffix = matchValue_4[1];
                    const modifiers_3 = matchValue_4[0];
                    return append(singleton_1(move("editor.selection.box.west" + suffix, "ArrowLeft", modifiers_3, -1, 0)), delay(() => append(singleton_1(move("editor.selection.box.east" + suffix, "ArrowRight", modifiers_3, 1, 0)), delay(() => append(singleton_1(move("editor.selection.box.north" + suffix, "ArrowUp", modifiers_3, 0, -1)), delay(() => singleton_1(move("editor.selection.box.south" + suffix, "ArrowDown", modifiers_3, 0, 1))))))));
                }, [[ModalInput_plain, ""], [new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), "-extended"]]), delay(() => singleton_1(ModalInput_binding("editor.selection.box.add", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.BoxSelection])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Enter", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), InputPhase.KeyDown), "Add enclosed units to the selection", "Selection", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* AddEditorUnitsInBox */ 11, [new EditorBox(anchor.CellColumn, anchor.CellRow, current.CellColumn, current.CellRow)])])))))))))) : ((matchValue_3.tag === 5) ? ((extend = ((id_5, value_4, modifiers_4, dx_1, dy_1) => {
                    let matchValue_5;
                    return ModalInput_binding(id_5, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [(matchValue_5 = facts.Editor.Gesture, (matchValue_5.tag === 5) ? (new EditorGestureKind(/* TerrainPreview */ 4, [matchValue_5.fields[0]])) : (new EditorGestureKind(/* TerrainPreview */ 4, [TerrainAuthoringTool.PencilTool])))])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key(value_4, modifiers_4, InputPhase.KeyDown), "Move the terrain endpoint", "Terrain", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveTerrainCursor */ 3, [dx_1, dy_1, true])]));
                }), append(collect((matchValue_6) => {
                    const suffix_1 = matchValue_6[1];
                    const modifiers_5 = matchValue_6[0];
                    return append(singleton_1(extend("editor.terrain.gesture.west" + suffix_1, "ArrowLeft", modifiers_5, -1, 0)), delay(() => append(singleton_1(extend("editor.terrain.gesture.east" + suffix_1, "ArrowRight", modifiers_5, 1, 0)), delay(() => append(singleton_1(extend("editor.terrain.gesture.north" + suffix_1, "ArrowUp", modifiers_5, 0, -1)), delay(() => singleton_1(extend("editor.terrain.gesture.south" + suffix_1, "ArrowDown", modifiers_5, 0, 1))))))));
                }, [[ModalInput_plain, ""], [new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), "-extended"]]), delay(() => {
                    let matchValue_7;
                    return singleton_1(ModalInput_binding("editor.terrain.gesture.reset", new ModalContextSelector(/* ExactContext */ 2, [(matchValue_7 = facts.Editor.Gesture, (matchValue_7.tag === 5) ? (new ModalContext(/* EditorGesture */ 3, [new EditorGestureKind(/* TerrainPreview */ 4, [matchValue_7.fields[0]])])) : (new ModalContext(/* EditorGesture */ 3, [new EditorGestureKind(/* TerrainPreview */ 4, [TerrainAuthoringTool.PencilTool])])))]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Backspace", ModalInput_plain, InputPhase.KeyDown), "Reset endpoint to anchor", "Terrain", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ResetTerrainPreview])));
                })))) : ((matchValue_3.tag === 4) ? ((current_1 = matchValue_3.fields[1], (extend_1 = ((id_6, value_5, modifiers_6, distance, dx_2, dy_2) => ModalInput_binding(id_6, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.UnitMovePreview])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key(value_5, modifiers_6, InputPhase.KeyDown), "Move the formation preview", "Units", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ExtendUnitMove */ 58, [new EditorCellAddress(max_1(0, min_1(facts.Editor.Map.Width - 1, current_1.CellColumn + (dx_2 * distance))), max_1(0, min_1(facts.Editor.Map.Height - 1, current_1.CellRow + (dy_2 * distance))))])]))), append(collect((matchValue_8) => {
                    const suffix_2 = matchValue_8[2];
                    const modifiers_7 = matchValue_8[0];
                    const distance_1 = matchValue_8[1] | 0;
                    return append(singleton_1(extend_1("editor.unit.move.west" + suffix_2, "ArrowLeft", modifiers_7, distance_1, -1, 0)), delay(() => append(singleton_1(extend_1("editor.unit.move.east" + suffix_2, "ArrowRight", modifiers_7, distance_1, 1, 0)), delay(() => append(singleton_1(extend_1("editor.unit.move.north" + suffix_2, "ArrowUp", modifiers_7, distance_1, 0, -1)), delay(() => singleton_1(extend_1("editor.unit.move.south" + suffix_2, "ArrowDown", modifiers_7, distance_1, 0, 1))))))));
                }, [[ModalInput_plain, 1, ""], [new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 5, "-large"]]), delay(() => singleton_1(ModalInput_binding("editor.unit.move.reset", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.UnitMovePreview])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Backspace", ModalInput_plain, InputPhase.KeyDown), "Reset movement preview", "Units", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ResetUnitMovePreview])))))))) : (empty_2()))))), delay(() => {
                    let patternInput, edgeRow, edgeDirection, edgeColumn, edgeTool, matchValue_9, edgeBinding;
                    return append(equals(facts.ActiveDomain, EditorDomain.EdgeDomain) ? ((patternInput = facts.Editor.EdgeCursor, (edgeRow = (patternInput[1] | 0), (edgeDirection = patternInput[2], (edgeColumn = (patternInput[0] | 0), (edgeTool = ((matchValue_9 = facts.Editor.Tool, (matchValue_9.tag === 5) ? [matchValue_9.fields[0], matchValue_9.fields[1]] : [edgeDirection, MapEdgeKind.Wall])), (edgeBinding = ((id_7, value_6, modifiers_8, label_1, repeat_1, command_1) => ModalInput_binding(id_7, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorDomain */ 1, [EditorDomain.EdgeDomain])]), ModalPrecedence.ActiveTool, ModalInput_key(value_6, modifiers_8, InputPhase.KeyDown), label_1, "Edges", repeat_1, ModalInput_available, command_1)), append(singleton_1(edgeBinding("editor.edge.kind.wall", "w", ModalInput_plain, "Choose wall", RepeatPolicy.IgnoreRepeat, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ConvertEdge */ 17, [edgeColumn, edgeRow, edgeDirection, MapEdgeKind.Wall])]))), delay(() => append(singleton_1(edgeBinding("editor.edge.kind.door", "d", ModalInput_plain, "Choose closed door", RepeatPolicy.IgnoreRepeat, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ConvertEdge */ 17, [edgeColumn, edgeRow, edgeDirection, MapEdgeKind.Door])]))), delay(() => append(singleton_1(edgeBinding("editor.edge.kind.window", "n", ModalInput_plain, "Choose window", RepeatPolicy.IgnoreRepeat, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ConvertEdge */ 17, [edgeColumn, edgeRow, edgeDirection, MapEdgeKind.Window$])]))), delay(() => append(singleton_1(edgeBinding("editor.edge.orientation.rotate", "r", ModalInput_plain, "Rotate edge orientation", RepeatPolicy.IgnoreRepeat, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [new MapEditorTool(/* Edge */ 5, [equals(edgeTool[0], MapEdgeDirection.EastEdge) ? MapEdgeDirection.SouthEdge : MapEdgeDirection.EastEdge, edgeTool[1]])])]))), delay(() => {
                        const edgeMove = (id_8, value_7, modifiers_9, dx_3, dy_3, extend_2) => edgeBinding(id_8, value_7, modifiers_9, extend_2 ? "Extend the wall polyline" : "Move the snapped edge cursor", RepeatPolicy.AllowRepeat, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveEdgeCursor */ 13, [dx_3, dy_3, extend_2])]));
                        return append(singleton_1(edgeMove("editor.edge.cursor.west", "ArrowLeft", ModalInput_plain, -1, 0, false)), delay(() => append(singleton_1(edgeMove("editor.edge.cursor.east", "ArrowRight", ModalInput_plain, 1, 0, false)), delay(() => append(singleton_1(edgeMove("editor.edge.cursor.north", "ArrowUp", ModalInput_plain, 0, -1, false)), delay(() => append(singleton_1(edgeMove("editor.edge.cursor.south", "ArrowDown", ModalInput_plain, 0, 1, false)), delay(() => append(singleton_1(edgeMove("editor.edge.polyline.west", "ArrowLeft", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), -1, 0, true)), delay(() => append(singleton_1(edgeMove("editor.edge.polyline.east", "ArrowRight", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 1, 0, true)), delay(() => append(singleton_1(edgeMove("editor.edge.polyline.north", "ArrowUp", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 0, -1, true)), delay(() => append(singleton_1(edgeMove("editor.edge.polyline.south", "ArrowDown", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 0, 1, true)), delay(() => append(equals(facts.Editor.Gesture, EditorGesture.IdleGesture) ? singleton_1(edgeBinding("editor.edge.activate", "Enter", ModalInput_plain, "Apply the selected edge or begin a wall polyline", RepeatPolicy.IgnoreRepeat, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ActivateEdgeCursor]))) : ((facts.Editor.Gesture.tag === 6) ? singleton_1(ModalInput_binding("editor.edge.polyline.backtrack", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.EdgePolyline])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Backspace", ModalInput_plain, InputPhase.KeyDown), "Remove the last polyline segment", "Edges", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.BacktrackEdgePolyline]))) : (empty_2())), delay(() => append(singleton_1(edgeBinding("editor.edge.door.toggle", "o", ModalInput_plain, "Toggle door open or closed", RepeatPolicy.IgnoreRepeat, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ToggleDoorState */ 18, [edgeColumn, edgeRow, edgeDirection])]))), delay(() => append(singleton_1(edgeBinding("editor.edge.erase", "x", ModalInput_plain, "Erase the cursor edge", RepeatPolicy.IgnoreRepeat, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* EraseEdge */ 19, [edgeColumn, edgeRow, edgeDirection])]))), delay(() => append(singleton_1(edgeBinding("editor.edge.split", "s", ModalInput_plain, "Split the edge run", RepeatPolicy.IgnoreRepeat, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* SplitEdge */ 20, [edgeColumn, edgeRow, edgeDirection])]))), delay(() => singleton_1(edgeBinding("editor.edge.join", "j", ModalInput_plain, "Join a compatible edge run", RepeatPolicy.IgnoreRepeat, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* JoinEdge */ 21, [edgeColumn, edgeRow, edgeDirection])])))))))))))))))))))))))))));
                    }))))))))))))))) : empty_2(), delay(() => {
                        let regionBinding, cursorMove, previewMove, matchValue_11, context_4, context_5, anchor_1, context_6, vertices, context_7, context_8, context_9, context_10, context_3, idle;
                        return append(equals(facts.ActiveDomain, EditorDomain.RegionDomain) ? ((regionBinding = ((id_9, context, value_8, modifiers_10, label_2, repeat_2, availability_1, command_2) => ModalInput_binding(id_9, context, ModalPrecedence.ActiveGestureOrPreview, ModalInput_key(value_8, modifiers_10, InputPhase.KeyDown), label_2, "Zones", repeat_2, availability_1, command_2)), (cursorMove = ((id_10, context_1, value_9, modifiers_11, dx_4, dy_4) => regionBinding(id_10, context_1, value_9, modifiers_11, "Move the region cursor", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveRegionCursor */ 32, [dx_4, dy_4])]))), (previewMove = ((id_11, context_2, value_10, modifiers_12, dx_5, dy_5, opposite) => {
                            const distance_2 = ((modifiers_12.Shift && !opposite) ? 5 : 1) | 0;
                            return regionBinding(id_11, context_2, value_10, modifiers_12, "Update the region preview", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveRegionEditPreview */ 43, [dx_5 * distance_2, dy_5 * distance_2, opposite])]));
                        }), (matchValue_11 = facts.Editor.RegionKeyboardMode, (matchValue_11.tag === 1) ? ((context_4 = (new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionPurpose])])), append(collect((matchValue_13) => singleton_1(regionBinding("editor.region.purpose." + matchValue_13[0], context_4, matchValue_13[1], ModalInput_plain, "Choose " + matchValue_13[3], RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseRegionPurpose */ 34, [matchValue_13[2]])]))), [["objective", "o", RegionPurpose.ObjectiveRegion, "Objective"], ["blue", "b", new RegionPurpose(/* DeploymentZone */ 1, [MapSide.Blue]), "Blue deployment"], ["red", "r", new RegionPurpose(/* DeploymentZone */ 1, [MapSide.Red]), "Red deployment"]]), delay(() => append(matchValue_11.fields[0] ? singleton_1(regionBinding("editor.region.purpose.commit", context_4, "Enter", ModalInput_plain, "Apply the highlighted purpose", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CommitRegionEditPreview]))) : empty_2(), delay(() => singleton_1(regionBinding("editor.region.purpose.cancel", context_4, "Escape", ModalInput_plain, "Cancel purpose selection", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelRegionKeyboardMode]))))))))) : ((matchValue_11.tag === 2) ? ((context_5 = (new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionShape])])), append(singleton_1(regionBinding("editor.region.shape.rectangle", context_5, "r", ModalInput_plain, "Choose rectangle geometry", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseRegionShape */ 35, [RegionShape.RectangleRegionShape])]))), delay(() => append(singleton_1(regionBinding("editor.region.shape.polygon", context_5, "p", ModalInput_plain, "Choose polygon geometry", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseRegionShape */ 35, [RegionShape.PolygonRegionShape])]))), delay(() => singleton_1(regionBinding("editor.region.shape.back", context_5, "Escape", ModalInput_plain, "Return to purpose selection", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelRegionKeyboardMode]))))))))) : ((matchValue_11.tag === 3) ? ((anchor_1 = matchValue_11.fields[1], (context_6 = (new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionRectangleMode])])), append(collect((matchValue_14) => singleton_1(cursorMove("editor.region.rectangle." + matchValue_14[0], context_6, matchValue_14[1], ModalInput_plain, matchValue_14[2], matchValue_14[3])), [["west", "ArrowLeft", -1, 0], ["east", "ArrowRight", 1, 0], ["north", "ArrowUp", 0, -1], ["south", "ArrowDown", 0, 1]]), delay(() => append(singleton_1(regionBinding("editor.region.rectangle.activate", context_6, "Enter", ModalInput_plain, (anchor_1 != null) ? "Commit the rectangle" : "Set the first rectangle corner", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ActivateRegionCursor]))), delay(() => append(singleton_1(regionBinding("editor.region.rectangle.reset", context_6, "Backspace", ModalInput_plain, "Clear the first rectangle corner", RepeatPolicy.IgnoreRepeat, (anchor_1 != null) ? (ModalInput_available) : ((_arg_9) => (new BindingAvailability(/* Unavailable */ 1, ["No rectangle corner is set."]))), new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.BacktrackRegionConstruction]))), delay(() => singleton_1(regionBinding("editor.region.rectangle.cancel", context_6, "Escape", ModalInput_plain, "Cancel rectangle geometry", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelRegionKeyboardMode])))))))))))) : ((matchValue_11.tag === 4) ? ((vertices = matchValue_11.fields[1], (context_7 = (new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionPolygonMode])])), append(collect((matchValue_15) => singleton_1(cursorMove("editor.region.polygon." + matchValue_15[0], context_7, matchValue_15[1], ModalInput_plain, matchValue_15[2], matchValue_15[3])), [["west", "ArrowLeft", -1, 0], ["east", "ArrowRight", 1, 0], ["north", "ArrowUp", 0, -1], ["south", "ArrowDown", 0, 1]]), delay(() => append(singleton_1(regionBinding("editor.region.polygon.vertex", context_7, "Enter", ModalInput_plain, "Add a polygon vertex", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ActivateRegionCursor]))), delay(() => append(singleton_1(regionBinding("editor.region.polygon.commit", context_7, "Enter", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), "Close and commit the polygon", RepeatPolicy.IgnoreRepeat, (vertices.length >= 3) ? (ModalInput_available) : ((_arg_10) => (new BindingAvailability(/* Unavailable */ 1, ["Add at least three vertices."]))), new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CommitRegionPolygon]))), delay(() => append(singleton_1(regionBinding("editor.region.polygon.backtrack", context_7, "Backspace", ModalInput_plain, "Remove the last polygon vertex", RepeatPolicy.IgnoreRepeat, (vertices.length === 0) ? ((_arg_11) => (new BindingAvailability(/* Unavailable */ 1, ["No polygon vertex is staged."]))) : (ModalInput_available), new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.BacktrackRegionConstruction]))), delay(() => singleton_1(regionBinding("editor.region.polygon.cancel", context_7, "Escape", ModalInput_plain, "Cancel polygon geometry", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelRegionKeyboardMode])))))))))))))) : ((matchValue_11.tag === 5) ? ((context_8 = (new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionMove])])), append(collect((matchValue_16) => {
                            const distance_3 = matchValue_16[1] | 0;
                            return collect((matchValue_17) => singleton_1(regionBinding(("editor.region.move." + matchValue_17[0]) + matchValue_16[2], context_8, matchValue_17[1], matchValue_16[0], "Move the region preview", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveRegionEditPreview */ 43, [matchValue_17[2] * distance_3, matchValue_17[3] * distance_3, false])]))), [["west", "ArrowLeft", -1, 0], ["east", "ArrowRight", 1, 0], ["north", "ArrowUp", 0, -1], ["south", "ArrowDown", 0, 1]]);
                        }, [[ModalInput_plain, 1, ""], [new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 5, "-large"]]), delay(() => append(singleton_1(regionBinding("editor.region.move.commit", context_8, "Enter", ModalInput_plain, "Commit the region move", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CommitRegionEditPreview]))), delay(() => append(singleton_1(regionBinding("editor.region.move.reset", context_8, "Backspace", ModalInput_plain, "Reset the region move", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ResetRegionEditPreview]))), delay(() => singleton_1(regionBinding("editor.region.move.cancel", context_8, "Escape", ModalInput_plain, "Cancel the region move", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelRegionKeyboardMode]))))))))))) : ((matchValue_11.tag === 6) ? ((context_9 = (new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionResize])])), append(singleton_1(previewMove("editor.region.resize.width.decrease", context_9, "ArrowLeft", ModalInput_plain, -1, 0, false)), delay(() => append(singleton_1(previewMove("editor.region.resize.width.increase", context_9, "ArrowRight", ModalInput_plain, 1, 0, false)), delay(() => append(singleton_1(previewMove("editor.region.resize.height.decrease", context_9, "ArrowUp", ModalInput_plain, 0, -1, false)), delay(() => append(singleton_1(previewMove("editor.region.resize.height.increase", context_9, "ArrowDown", ModalInput_plain, 0, 1, false)), delay(() => append(singleton_1(previewMove("editor.region.resize.origin.east", context_9, "ArrowLeft", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 1, 0, true)), delay(() => append(singleton_1(previewMove("editor.region.resize.origin.west", context_9, "ArrowRight", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), -1, 0, true)), delay(() => append(singleton_1(previewMove("editor.region.resize.origin.south", context_9, "ArrowUp", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 0, 1, true)), delay(() => append(singleton_1(previewMove("editor.region.resize.origin.north", context_9, "ArrowDown", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 0, -1, true)), delay(() => append(singleton_1(regionBinding("editor.region.resize.commit", context_9, "Enter", ModalInput_plain, "Commit the rectangle resize", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CommitRegionEditPreview]))), delay(() => append(singleton_1(regionBinding("editor.region.resize.reset", context_9, "Backspace", ModalInput_plain, "Reset the rectangle resize", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ResetRegionEditPreview]))), delay(() => singleton_1(regionBinding("editor.region.resize.cancel", context_9, "Escape", ModalInput_plain, "Cancel the rectangle resize", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelRegionKeyboardMode]))))))))))))))))))))))))) : ((matchValue_11.tag === 7) ? ((context_10 = (new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorGesture */ 3, [EditorGestureKind.RegionVertex])])), append(singleton_1(regionBinding("editor.region.vertex.previous", context_10, "[", ModalInput_plain, "Previous polygon vertex", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* CycleRegionVertex */ 44, [-1])]))), delay(() => append(singleton_1(regionBinding("editor.region.vertex.next", context_10, "]", ModalInput_plain, "Next polygon vertex", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* CycleRegionVertex */ 44, [1])]))), delay(() => append(collect((matchValue_18) => {
                            const distance_4 = matchValue_18[1] | 0;
                            return collect((matchValue_19) => singleton_1(regionBinding(("editor.region.vertex." + matchValue_19[0]) + matchValue_18[2], context_10, matchValue_19[1], matchValue_18[0], "Move the active polygon vertex", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveRegionEditPreview */ 43, [matchValue_19[2] * distance_4, matchValue_19[3] * distance_4, false])]))), [["west", "ArrowLeft", -1, 0], ["east", "ArrowRight", 1, 0], ["north", "ArrowUp", 0, -1], ["south", "ArrowDown", 0, 1]]);
                        }, [[ModalInput_plain, 1, ""], [new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), 5, "-large"]]), delay(() => append(singleton_1(regionBinding("editor.region.vertex.commit", context_10, "Enter", ModalInput_plain, "Commit polygon vertex edits", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CommitRegionEditPreview]))), delay(() => append(singleton_1(regionBinding("editor.region.vertex.reset", context_10, "Backspace", ModalInput_plain, "Reset the active polygon vertex", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ResetRegionEditPreview]))), delay(() => singleton_1(regionBinding("editor.region.vertex.cancel", context_10, "Escape", ModalInput_plain, "Cancel polygon vertex edits", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelRegionKeyboardMode]))))))))))))))) : ((context_3 = (new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorDomain */ 1, [EditorDomain.RegionDomain])])), (idle = ((id_12, value_11, label_3, repeat_3, availability_2, command_3) => ModalInput_binding(id_12, context_3, ModalPrecedence.ActiveGestureOrPreview, ModalInput_key(value_11, ModalInput_plain, InputPhase.KeyDown), label_3, "Zones", repeat_3, availability_2, command_3)), append(collect((matchValue_12) => singleton_1(ModalInput_binding("editor.region.cursor." + matchValue_12[0], context_3, ModalPrecedence.ActiveGestureOrPreview, ModalInput_key(matchValue_12[1], ModalInput_plain, InputPhase.KeyDown), "Move the region cursor", "Zones", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveRegionCursor */ 32, [matchValue_12[2], matchValue_12[3]])]))), [["west", "ArrowLeft", -1, 0], ["east", "ArrowRight", 1, 0], ["north", "ArrowUp", 0, -1], ["south", "ArrowDown", 0, 1]]), delay(() => append(singleton_1(idle("editor.region.select", "Enter", "Select the region under the cursor", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ActivateRegionCursor]))), delay(() => append(singleton_1(idle("editor.region.create.begin", "n", "Begin a new region", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.BeginNewRegion]))), delay(() => append(singleton_1(idle("editor.region.edit.move", "m", "Move the selected region", RepeatPolicy.IgnoreRepeat, selectedRegionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.BeginSelectedRegionMove]))), delay(() => append(singleton_1(idle("editor.region.edit.resize", "r", "Resize the selected rectangle", RepeatPolicy.IgnoreRepeat, (_arg_7) => {
                            let matchValue;
                            const option_1 = facts.Editor.SelectedRegion;
                            matchValue = ((option_1 != null) ? tryFind_1(option_1, facts.Editor.Map.Regions) : undefined);
                            if (matchValue == null) {
                                return new BindingAvailability(/* Unavailable */ 1, ["Select a region first."]);
                            }
                            else if (matchValue.Geometry.tag === 0) {
                                return BindingAvailability.Available;
                            }
                            else {
                                return new BindingAvailability(/* Unavailable */ 1, ["The selected region is not a rectangle."]);
                            }
                        }, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.BeginSelectedRegionResize]))), delay(() => append(singleton_1(idle("editor.region.edit.vertices", "v", "Edit selected polygon vertices", RepeatPolicy.IgnoreRepeat, (_arg_8) => {
                            let matchValue_1;
                            const option_3 = facts.Editor.SelectedRegion;
                            matchValue_1 = ((option_3 != null) ? tryFind_1(option_3, facts.Editor.Map.Regions) : undefined);
                            if (matchValue_1 == null) {
                                return new BindingAvailability(/* Unavailable */ 1, ["Select a region first."]);
                            }
                            else if (matchValue_1.Geometry.tag === 1) {
                                return BindingAvailability.Available;
                            }
                            else {
                                return new BindingAvailability(/* Unavailable */ 1, ["The selected region is not a polygon."]);
                            }
                        }, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.BeginSelectedRegionVertexEdit]))), delay(() => append(singleton_1(idle("editor.region.edit.purpose", "p", "Change selected region purpose", RepeatPolicy.IgnoreRepeat, selectedRegionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.BeginSelectedRegionPurposeEdit]))), delay(() => append(singleton_1(idle("editor.region.delete", "Delete", "Delete the selected region", RepeatPolicy.IgnoreRepeat, selectedRegionAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.RemoveSelectedRegion]))), delay(() => singleton_1(idle("editor.region.exit", "Escape", (facts.Editor.SelectedRegion != null) ? "Clear the region selection" : "Return to Select", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.CancelRegionKeyboardMode]))))))))))))))))))))))))))))))))) : empty_2(), delay(() => {
                            let document$;
                            return append(equals(facts.ActiveDomain, EditorDomain.DocumentDomain) ? ((document$ = ((id_19, value_18, label_4, command_4) => ModalInput_binding(id_19, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorDomain */ 1, [EditorDomain.DocumentDomain])]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key(value_18, ModalInput_plain, InputPhase.KeyDown), label_4, "Document", RepeatPolicy.IgnoreRepeat, ModalInput_available, command_4)), append(singleton_1(document$("editor.document.new", "n", "Request a new map", new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.RequestNewMap]))), delay(() => append(singleton_1(document$("editor.document.clear", "c", "Request clearing the map", new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.RequestClearMap]))), delay(() => append(singleton_1(document$("editor.document.export", "s", "Save or export the canonical map", new ModalCommand(/* EditorDocumentCommand */ 15, [EditorDocumentCommand.ExportMapDocument]))), delay(() => append(singleton_1(document$("editor.document.import", "i", "Open the native map import picker", new ModalCommand(/* EditorDocumentCommand */ 15, [EditorDocumentCommand.OpenMapImport]))), delay(() => append(singleton_1(document$("editor.document.bundle", "b", "Export the repository design bundle", new ModalCommand(/* EditorDocumentCommand */ 15, [EditorDocumentCommand.ExportRepositoryDesignBundle]))), delay(() => append(singleton_1(document$("editor.document.layers", "l", "Focus layer-state controls", new ModalCommand(/* EditorDocumentCommand */ 15, [new EditorDocumentCommand(/* FocusDocumentControl */ 3, [EditorDocumentControl.LayerStateControls])]))), delay(() => append(singleton_1(document$("editor.document.background", "g", "Focus local background controls", new ModalCommand(/* EditorDocumentCommand */ 15, [new EditorDocumentCommand(/* FocusDocumentControl */ 3, [EditorDocumentControl.LocalBackgroundControls])]))), delay(() => append(singleton_1(document$("editor.document.resize", "r", "Focus map dimensions", new ModalCommand(/* EditorDocumentCommand */ 15, [new EditorDocumentCommand(/* FocusDocumentControl */ 3, [EditorDocumentControl.MapDimensionControls])]))), delay(() => append(singleton_1(document$("editor.document.views", "v", "Focus saved views", new ModalCommand(/* EditorDocumentCommand */ 15, [new EditorDocumentCommand(/* FocusDocumentControl */ 3, [EditorDocumentControl.SavedViewControls])]))), delay(() => singleton_1(document$("editor.document.exit", "Escape", "Return to Select", new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [MapEditorTool.Select])]))))))))))))))))))))))) : empty_2(), delay(() => {
                                const matchValue_20 = facts.Editor.Tool;
                                let matchResult;
                                switch (matchValue_20.tag) {
                                    case 0: {
                                        if (equals(facts.Editor.Gesture, EditorGesture.IdleGesture)) {
                                            matchResult = 0;
                                        }
                                        else {
                                            matchResult = 4;
                                        }
                                        break;
                                    }
                                    case 3: {
                                        matchResult = 1;
                                        break;
                                    }
                                    case 4: {
                                        if (equals(facts.Editor.Gesture, EditorGesture.IdleGesture)) {
                                            matchResult = 2;
                                        }
                                        else {
                                            matchResult = 4;
                                        }
                                        break;
                                    }
                                    case 2: {
                                        matchResult = 3;
                                        break;
                                    }
                                    default:
                                        matchResult = 4;
                                }
                                switch (matchResult) {
                                    case 0: {
                                        const movement = (id_20, value_19, dx_11, dy_11) => ModalInput_binding(id_20, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.Select])]), ModalPrecedence.ActiveTool, ModalInput_key(value_19, ModalInput_plain, InputPhase.KeyDown), "Move the map cursor", "Selection", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveEditorKeyboardCursor */ 6, [dx_11, dy_11])]));
                                        return append(singleton_1(movement("editor.cursor.west", "ArrowLeft", -1, 0)), delay(() => append(singleton_1(movement("editor.cursor.east", "ArrowRight", 1, 0)), delay(() => append(singleton_1(movement("editor.cursor.north", "ArrowUp", 0, -1)), delay(() => append(singleton_1(movement("editor.cursor.south", "ArrowDown", 0, 1)), delay(() => append(singleton_1(ModalInput_binding("editor.selection.single", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.Select])]), ModalPrecedence.ActiveTool, ModalInput_key("Enter", ModalInput_plain, InputPhase.KeyDown), "Select the current object", "Selection", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ActivateEditorKeyboardCursor */ 8, [false])]))), delay(() => append(singleton_1(ModalInput_binding("editor.selection.toggle", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.Select])]), ModalPrecedence.ActiveTool, ModalInput_key("Enter", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), InputPhase.KeyDown), "Toggle the current object in the selection", "Selection", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ActivateEditorKeyboardCursor */ 8, [true])]))), delay(() => append(singleton_1(ModalInput_binding("editor.cursor.next-object", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.Select])]), ModalPrecedence.ActiveTool, ModalInput_key("n", ModalInput_plain, InputPhase.KeyDown), "Select the next object at the cursor", "Selection", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* CycleEditorKeyboardObject */ 7, [1])]))), delay(() => append(singleton_1(ModalInput_binding("editor.cursor.previous-object", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.Select])]), ModalPrecedence.ActiveTool, ModalInput_key("p", ModalInput_plain, InputPhase.KeyDown), "Select the previous object at the cursor", "Selection", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* CycleEditorKeyboardObject */ 7, [-1])]))), delay(() => append(singleton_1(ModalInput_binding("editor.selection.box.begin", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.Select])]), ModalPrecedence.ActiveTool, ModalInput_key("b", ModalInput_plain, InputPhase.KeyDown), "Begin box selection", "Selection", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.BeginKeyboardBoxSelection]))), delay(() => append(singleton_1(ModalInput_binding("editor.selection.all-domain", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.Select])]), ModalPrecedence.ActiveTool, ModalInput_key("a", ModalInput_plain, InputPhase.KeyDown), "Select all units", "Selection", RepeatPolicy.IgnoreRepeat, selectableDomainAvailable, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.SelectAllInActiveDomain]))), delay(() => singleton_1(ModalInput_binding("editor.unit.move.begin", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.Select])]), ModalPrecedence.ActiveTool, ModalInput_key("m", ModalInput_plain, InputPhase.KeyDown), "Begin moving selected units", "Units", RepeatPolicy.IgnoreRepeat, unitSelectionAvailable, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* BeginUnitMove */ 57, [facts.Editor.KeyboardCursor.Cell])])))))))))))))))))))))));
                                    }
                                    case 1: {
                                        const browse = (id_21, value_20, repeat_4, delta) => ModalInput_binding(id_21, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.UnitBrowse])]), ModalPrecedence.ActiveTool, ModalInput_key(value_20, ModalInput_plain, InputPhase.KeyDown), "Browse unit presets", "Units", repeat_4, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveUnitPaletteCursor */ 23, [delta])]));
                                        return append(singleton_1(browse("editor.unit.preset.previous-arrow", "ArrowUp", RepeatPolicy.AllowRepeat, -1)), delay(() => append(singleton_1(browse("editor.unit.preset.next-arrow", "ArrowDown", RepeatPolicy.AllowRepeat, 1)), delay(() => append(singleton_1(browse("editor.unit.preset.previous-bracket", "[", RepeatPolicy.AllowRepeat, -1)), delay(() => append(singleton_1(browse("editor.unit.preset.next-bracket", "]", RepeatPolicy.AllowRepeat, 1)), delay(() => append(singleton_1(ModalInput_binding("editor.unit.preset.previous-faction", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.UnitBrowse])]), ModalPrecedence.ActiveTool, ModalInput_key("PageUp", ModalInput_plain, InputPhase.KeyDown), "Previous faction group", "Units", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* PageUnitPaletteFaction */ 24, [-1])]))), delay(() => append(singleton_1(ModalInput_binding("editor.unit.preset.next-faction", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.UnitBrowse])]), ModalPrecedence.ActiveTool, ModalInput_key("PageDown", ModalInput_plain, InputPhase.KeyDown), "Next faction group", "Units", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* PageUnitPaletteFaction */ 24, [1])]))), delay(() => append(singleton_1(ModalInput_binding("editor.unit.preset.first", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.UnitBrowse])]), ModalPrecedence.ActiveTool, ModalInput_key("Home", ModalInput_plain, InputPhase.KeyDown), "First visible preset", "Units", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* SelectUnitPaletteBoundary */ 25, [false])]))), delay(() => append(singleton_1(ModalInput_binding("editor.unit.preset.last", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.UnitBrowse])]), ModalPrecedence.ActiveTool, ModalInput_key("End", ModalInput_plain, InputPhase.KeyDown), "Last visible preset", "Units", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* SelectUnitPaletteBoundary */ 25, [true])]))), delay(() => append(singleton_1(ModalInput_binding("editor.unit.preset.arm", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.UnitBrowse])]), ModalPrecedence.ActiveTool, ModalInput_key("Enter", ModalInput_plain, InputPhase.KeyDown), "Arm highlighted preset", "Units", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ArmUnitPalettePreset]))), delay(() => append(singleton_1(ModalInput_binding("editor.unit.preset.search", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.UnitBrowse])]), ModalPrecedence.ActiveTool, ModalInput_key("/", ModalInput_plain, InputPhase.KeyDown), "Focus preset search", "Units", RepeatPolicy.IgnoreRepeat, ModalInput_available, ModalCommand.FocusUnitPresetSearch)), delay(() => singleton_1(ModalInput_binding("editor.unit.preset.exit", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [MapEditorTool.UnitBrowse])]), ModalPrecedence.ActiveTool, ModalInput_key("Escape", ModalInput_plain, InputPhase.KeyDown), "Return to Select", "Units", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTool */ 0, [MapEditorTool.Select])])))))))))))))))))))))));
                                    }
                                    case 2: {
                                        const move_1 = (id_22, value_21, dx_12, dy_12) => ModalInput_binding(id_22, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key(value_21, ModalInput_plain, InputPhase.KeyDown), "Move the placement cursor", "Units", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveUnitPlacementCursor */ 28, [dx_12, dy_12])]));
                                        return append(singleton_1(move_1("editor.unit.place.west", "ArrowLeft", -1, 0)), delay(() => append(singleton_1(move_1("editor.unit.place.east", "ArrowRight", 1, 0)), delay(() => append(singleton_1(move_1("editor.unit.place.north", "ArrowUp", 0, -1)), delay(() => append(singleton_1(move_1("editor.unit.place.south", "ArrowDown", 0, 1)), delay(() => append(singleton_1(ModalInput_binding("editor.unit.place.previous-preset", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key("[", ModalInput_plain, InputPhase.KeyDown), "Arm previous visible preset", "Units", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* CycleArmedUnitPreset */ 29, [-1])]))), delay(() => append(singleton_1(ModalInput_binding("editor.unit.place.next-preset", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key("]", ModalInput_plain, InputPhase.KeyDown), "Arm next visible preset", "Units", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* CycleArmedUnitPreset */ 29, [1])]))), delay(() => append(singleton_1(ModalInput_binding("editor.unit.place.commit", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key("Enter", ModalInput_plain, InputPhase.KeyDown), "Place and remain armed", "Units", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* CommitUnitPlacement */ 30, [false])]))), delay(() => append(singleton_1(ModalInput_binding("editor.unit.place.commit-return", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key("Enter", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), InputPhase.KeyDown), "Place and return to preset browse", "Units", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* CommitUnitPlacement */ 30, [true])]))), delay(() => collect((matchValue_21) => singleton_1(ModalInput_binding("editor.unit.place." + matchValue_21[1], new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key(matchValue_21[0], ModalInput_plain, InputPhase.KeyDown), "Return to unit preset browse", "Units", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ReturnToUnitBrowse]))), [["b", "browse"], ["Escape", "cancel"]])))))))))))))))));
                                    }
                                    case 3: {
                                        const movement_1 = (id_23, value_23, dx_13, dy_13) => ModalInput_binding(id_23, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key(value_23, ModalInput_plain, InputPhase.KeyDown), "Move the terrain cursor", "Terrain", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveTerrainCursor */ 3, [dx_13, dy_13, false])]));
                                        return append(singleton_1(movement_1("editor.terrain.cursor.west", "ArrowLeft", -1, 0)), delay(() => append(singleton_1(movement_1("editor.terrain.cursor.east", "ArrowRight", 1, 0)), delay(() => append(singleton_1(movement_1("editor.terrain.cursor.north", "ArrowUp", 0, -1)), delay(() => append(singleton_1(movement_1("editor.terrain.cursor.south", "ArrowDown", 0, 1)), delay(() => {
                                            const shiftedMovement = (id_24, value_24, dx_14, dy_14) => ModalInput_binding(id_24, new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key(value_24, new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), InputPhase.KeyDown), "Paint or extend through the moved cell", "Terrain", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* MoveTerrainCursor */ 3, [dx_14, dy_14, true])]));
                                            return append(singleton_1(shiftedMovement("editor.terrain.cursor.paint-west", "ArrowLeft", -1, 0)), delay(() => append(singleton_1(shiftedMovement("editor.terrain.cursor.paint-east", "ArrowRight", 1, 0)), delay(() => append(singleton_1(shiftedMovement("editor.terrain.cursor.paint-north", "ArrowUp", 0, -1)), delay(() => append(singleton_1(shiftedMovement("editor.terrain.cursor.paint-south", "ArrowDown", 0, 1)), delay(() => append(singleton_1(ModalInput_binding("editor.terrain.activate", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key("Enter", ModalInput_plain, InputPhase.KeyDown), "Activate at the terrain cursor", "Terrain", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [MapEditorAction.ActivateTerrainCursor]))), delay(() => append(collect((matchValue_22) => {
                                                const name_1 = matchValue_22[2];
                                                return singleton_1(ModalInput_binding("editor.terrain.value." + name_1.toLowerCase(), new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key(matchValue_22[0], ModalInput_plain, InputPhase.KeyDown), ("Choose " + name_1) + " terrain", "Terrain values", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* ChooseTerrain */ 1, [matchValue_22[1]])])));
                                            }, [["1", MapTerrain.Open, "Open"], ["2", MapTerrain.Rough, "Rough"], ["3", MapTerrain.Blocked, "Blocked"], ["4", MapTerrain.Objective, "Objective"]]), delay(() => append(singleton_1(ModalInput_binding("editor.terrain.brush.decrease", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key("[", ModalInput_plain, InputPhase.KeyDown), "Decrease brush size", "Terrain", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* SetTerrainBrushSize */ 2, [facts.Editor.BrushSize - 1])]))), delay(() => append(singleton_1(ModalInput_binding("editor.terrain.brush.increase", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key("]", ModalInput_plain, InputPhase.KeyDown), "Increase brush size", "Terrain", RepeatPolicy.AllowRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [new MapEditorAction(/* SetTerrainBrushSize */ 2, [facts.Editor.BrushSize + 1])]))), delay(() => {
                                                let matchValue_23;
                                                return equals(facts.Editor.Gesture, EditorGesture.IdleGesture) ? singleton_1(ModalInput_binding("editor.terrain.exit", new ModalContextSelector(/* ExactContext */ 2, [new ModalContext(/* EditorTool */ 2, [facts.Editor.Tool])]), ModalPrecedence.ActiveTool, ModalInput_key("Escape", ModalInput_plain, InputPhase.KeyDown), "Return to Select", "Terrain", RepeatPolicy.IgnoreRepeat, ModalInput_available, new ModalCommand(/* EditorCommand */ 0, [(matchValue_23 = facts.Editor.Tool, (matchValue_23.tag === 2) ? ((matchValue_23.fields[0].tag === 4) ? (new MapEditorAction(/* ChooseTool */ 0, [new MapEditorTool(/* Terrain */ 2, [facts.Editor.LastTerrainPaintTool])])) : (new MapEditorAction(/* ChooseTool */ 0, [MapEditorTool.Select]))) : (new MapEditorAction(/* ChooseTool */ 0, [MapEditorTool.Select])))]))) : empty_2();
                                            }))))))))))))))));
                                        }))))))));
                                    }
                                    default: {
                                        return empty_2();
                                    }
                                }
                            }));
                        }));
                    }));
                }));
            }));
        }))));
    }))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
}

export function ModalInput_simulatorCatalog(selectedUnitId, handoff, controllerSelection) {
    const simulatorKey = (id, value, label, group, repeat, availability, command) => ModalInput_binding(id, ModalContextSelector.AnySimulatorContext, ModalPrecedence.WorkspaceCommands, ModalInput_key(value, ModalInput_plain, InputPhase.KeyDown), label, group, repeat, availability, command);
    const popupInactive = (contexts) => {
        if (contains_1(ModalContext.SimulatorControllerSelection, contexts, {
            Equals: equals,
            GetHashCode: (x) => (safeHash(x) | 0),
        })) {
            return new BindingAvailability(/* Unavailable */ 1, ["Finish or cancel controller selection first."]);
        }
        else if (handoff != null) {
            return BindingAvailability.Available;
        }
        else {
            return new BindingAvailability(/* Unavailable */ 1, ["Correct the current map so a simulation can be maintained."]);
        }
    };
    const paused = (contexts_1) => {
        if (contains_1(ModalContext.SimulatorControllerSelection, contexts_1, {
            Equals: equals,
            GetHashCode: (x_1) => (safeHash(x_1) | 0),
        })) {
            return new BindingAvailability(/* Unavailable */ 1, ["Finish or cancel controller selection first."]);
        }
        else if (handoff == null) {
            return new BindingAvailability(/* Unavailable */ 1, ["Correct the current map so a simulation can be maintained."]);
        }
        else if (!handoff.IsRunning) {
            const simulator_1 = handoff;
            return BindingAvailability.Available;
        }
        else {
            return new BindingAvailability(/* Unavailable */ 1, ["This command is unavailable while running."]);
        }
    };
    const selectedPaused = (contexts_2) => {
        const matchValue = paused(contexts_2);
        let matchResult, reason;
        if (matchValue.tag === 1) {
            matchResult = 2;
            reason = matchValue.fields[0];
        }
        else if (selectedUnitId == null) {
            if (handoff != null) {
                matchResult = 1;
            }
            else {
                matchResult = 3;
            }
        }
        else if (handoff != null) {
            matchResult = 0;
        }
        else {
            matchResult = 3;
        }
        switch (matchResult) {
            case 0:
                return BindingAvailability.Available;
            case 1:
                return new BindingAvailability(/* Unavailable */ 1, ["Select a unit first."]);
            case 2:
                return new BindingAvailability(/* Unavailable */ 1, [reason]);
            default:
                return new BindingAvailability(/* Unavailable */ 1, ["Correct the current map so a simulation can be maintained."]);
        }
    };
    const hasUnits = (contexts_3) => {
        const matchValue_2 = popupInactive(contexts_3);
        if (matchValue_2.tag === 1) {
            return new BindingAvailability(/* Unavailable */ 1, [matchValue_2.fields[0]]);
        }
        else if (handoff != null) {
            if (!isEmpty_1(handoff.RuntimeMap.Units)) {
                const simulator_3 = handoff;
                return BindingAvailability.Available;
            }
            else {
                return new BindingAvailability(/* Unavailable */ 1, ["The simulator has no units."]);
            }
        }
        else {
            return new BindingAvailability(/* Unavailable */ 1, ["Correct the current map so a simulation can be maintained."]);
        }
    };
    const previewAvailable = (contexts_4) => {
        const matchValue_4 = paused(contexts_4);
        if (matchValue_4.tag === 1) {
            return new BindingAvailability(/* Unavailable */ 1, [matchValue_4.fields[0]]);
        }
        else if (handoff != null) {
            if (handoff.PreviewDestination != null) {
                const simulator_5 = handoff;
                return BindingAvailability.Available;
            }
            else {
                return new BindingAvailability(/* Unavailable */ 1, ["No route preview is active."]);
            }
        }
        else {
            return new BindingAvailability(/* Unavailable */ 1, ["Correct the current map so a simulation can be maintained."]);
        }
    };
    const controllerActive = (contexts_5) => {
        let matchResult_1, simulator_7;
        if (controllerSelection == null) {
            matchResult_1 = 1;
        }
        else if (selectedUnitId == null) {
            if (handoff == null) {
                matchResult_1 = 4;
            }
            else {
                matchResult_1 = 2;
            }
        }
        else if (handoff == null) {
            matchResult_1 = 4;
        }
        else if (!handoff.IsRunning) {
            matchResult_1 = 0;
            simulator_7 = handoff;
        }
        else {
            matchResult_1 = 3;
        }
        switch (matchResult_1) {
            case 0:
                return BindingAvailability.Available;
            case 1:
                return new BindingAvailability(/* Unavailable */ 1, ["Controller selection is not active."]);
            case 2:
                return new BindingAvailability(/* Unavailable */ 1, ["Select a unit first."]);
            case 3:
                return new BindingAvailability(/* Unavailable */ 1, ["Controller mutation is unavailable while running."]);
            default:
                return new BindingAvailability(/* Unavailable */ 1, ["Correct the current map so a simulation can be maintained."]);
        }
    };
    return append_1(toList(delay(() => append(singleton_1(ModalInput_binding("simulator.help.toggle", ModalContextSelector.AnySimulatorContext, ModalPrecedence.WorkspaceCommands, ModalInput_key("?", new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), InputPhase.KeyDown), "Show or hide possible inputs", "Help", RepeatPolicy.IgnoreRepeat, ModalInput_available, ModalCommand.ToggleInputHelp)), delay(() => append(singleton_1(simulatorKey("simulator.panel.toggle", "F2", "Show or hide the active simulator panel", "Panels", RepeatPolicy.IgnoreRepeat, popupInactive, ModalCommand.ToggleSimulatorCommandPanel)), delay(() => append(singleton_1(simulatorKey("simulator.panel.controls", "c", "Show the Controls panel", "Panels", RepeatPolicy.IgnoreRepeat, popupInactive, new ModalCommand(/* ChooseSimulatorPanel */ 4, [SimulatorPanel.ControllerPanel]))), delay(() => append(singleton_1(simulatorKey("simulator.panel.events", "e", "Show the Events panel", "Panels", RepeatPolicy.IgnoreRepeat, popupInactive, new ModalCommand(/* ChooseSimulatorPanel */ 4, [SimulatorPanel.EventPanel]))), delay(() => append(singleton_1(simulatorKey("simulator.panel.samples", "a", "Show the Samples panel", "Panels", RepeatPolicy.IgnoreRepeat, popupInactive, new ModalCommand(/* ChooseSimulatorPanel */ 4, [SimulatorPanel.SimulatorSamplePanel]))), delay(() => append(singleton_1(simulatorKey("simulator.unit.previous", "[", "Inspect the previous unit", "Units", RepeatPolicy.IgnoreRepeat, hasUnits, new ModalCommand(/* TraverseSimulatorUnit */ 7, [-1]))), delay(() => append(singleton_1(simulatorKey("simulator.unit.next", "]", "Inspect the next unit", "Units", RepeatPolicy.IgnoreRepeat, hasUnits, new ModalCommand(/* TraverseSimulatorUnit */ 7, [1]))), delay(() => append(singleton_1(simulatorKey("simulator.run.toggle-space", "Space", "Start or pause the simulator", "Simulation", RepeatPolicy.IgnoreRepeat, popupInactive, new ModalCommand(/* SimulatorCommand */ 6, [SimulatorAction.ToggleSimulatorRun]))), delay(() => append(singleton_1(simulatorKey("simulator.run.toggle-k", "k", "Start or pause the simulator", "Simulation", RepeatPolicy.IgnoreRepeat, popupInactive, new ModalCommand(/* SimulatorCommand */ 6, [SimulatorAction.ToggleSimulatorRun]))), delay(() => append(singleton_1(simulatorKey("simulator.step", ".", "Advance exactly one deterministic tick", "Simulation", RepeatPolicy.IgnoreRepeat, paused, new ModalCommand(/* SimulatorCommand */ 6, [SimulatorAction.StepSimulator]))), delay(() => append(singleton_1(simulatorKey("simulator.reset.request", "r", "Reset the simulator sandbox", "Simulation", RepeatPolicy.IgnoreRepeat, paused, ModalCommand.RequestSimulatorSandboxReset)), delay(() => append(singleton_1(simulatorKey("simulator.controller.begin", "Enter", "Choose the selected unit controller", "Controllers", RepeatPolicy.IgnoreRepeat, selectedPaused, ModalCommand.BeginSimulatorControllerSelection)), delay(() => {
        const movement = (id_1, value_1, dx, dy) => simulatorKey(id_1, value_1, "Move the route-preview destination", "Route preview", RepeatPolicy.AllowRepeat, selectedPaused, new ModalCommand(/* SimulatorCommand */ 6, [new SimulatorAction(/* MoveSimulatorPreview */ 6, [dx, dy])]));
        return append(singleton_1(movement("simulator.preview.west", "ArrowLeft", -1, 0)), delay(() => append(singleton_1(movement("simulator.preview.east", "ArrowRight", 1, 0)), delay(() => append(singleton_1(movement("simulator.preview.north", "ArrowUp", 0, -1)), delay(() => append(singleton_1(movement("simulator.preview.south", "ArrowDown", 0, 1)), delay(() => {
            const fastMovement = (id_2, value_2, dx_1, dy_1) => ModalInput_binding(id_2, ModalContextSelector.AnySimulatorContext, ModalPrecedence.WorkspaceCommands, ModalInput_key(value_2, new KeyModifiers(ModalInput_plain.ControlOrMeta, true, ModalInput_plain.Alt), InputPhase.KeyDown), "Move the route-preview destination five cells", "Route preview", RepeatPolicy.AllowRepeat, selectedPaused, new ModalCommand(/* SimulatorCommand */ 6, [new SimulatorAction(/* MoveSimulatorPreview */ 6, [dx_1 * 5, dy_1 * 5])]));
            return append(singleton_1(fastMovement("simulator.preview.fast-west", "ArrowLeft", -1, 0)), delay(() => append(singleton_1(fastMovement("simulator.preview.fast-east", "ArrowRight", 1, 0)), delay(() => append(singleton_1(fastMovement("simulator.preview.fast-north", "ArrowUp", 0, -1)), delay(() => append(singleton_1(fastMovement("simulator.preview.fast-south", "ArrowDown", 0, 1)), delay(() => append(singleton_1(ModalInput_binding("simulator.preview.commit", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.SimulatorRoutePreview]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Enter", ModalInput_plain, InputPhase.KeyDown), "Commit the route preview", "Route preview", RepeatPolicy.IgnoreRepeat, previewAvailable, new ModalCommand(/* SimulatorCommand */ 6, [SimulatorAction.CommitSimulatorPreview]))), delay(() => append(singleton_1(ModalInput_binding("simulator.preview.reset", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.SimulatorRoutePreview]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Backspace", ModalInput_plain, InputPhase.KeyDown), "Return the route preview to the unit origin", "Route preview", RepeatPolicy.IgnoreRepeat, previewAvailable, new ModalCommand(/* SimulatorCommand */ 6, [SimulatorAction.ResetSimulatorPreviewToOrigin]))), delay(() => {
                const matchValue_7 = handoff;
                let matchResult_2, simulator_9;
                if (matchValue_7 != null) {
                    if (matchValue_7.PreviewDestination != null) {
                        matchResult_2 = 0;
                        simulator_9 = matchValue_7;
                    }
                    else {
                        matchResult_2 = 1;
                    }
                }
                else {
                    matchResult_2 = 1;
                }
                switch (matchResult_2) {
                    case 0:
                        return singleton_1(ModalInput_binding("simulator.preview.cancel", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.SimulatorRoutePreview]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Escape", ModalInput_plain, InputPhase.KeyDown), "Discard the route preview", "Route preview", RepeatPolicy.IgnoreRepeat, previewAvailable, new ModalCommand(/* SimulatorCommand */ 6, [SimulatorAction.ResetSimulatorPreview])));
                    default: {
                        return empty_2();
                    }
                }
            }))))))))))));
        }))))))));
    })))))))))))))))))))))))))), toList(delay(() => append(singleton_1(ModalInput_binding("simulator.controller.manual", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.SimulatorControllerSelection]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("m", ModalInput_plain, InputPhase.KeyDown), "Choose Manual controller", "Controller selection", RepeatPolicy.IgnoreRepeat, controllerActive, new ModalCommand(/* ChooseSimulatorController */ 9, [MapController.Manual]))), delay(() => append(singleton_1(ModalInput_binding("simulator.controller.scripted", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.SimulatorControllerSelection]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("s", ModalInput_plain, InputPhase.KeyDown), "Choose Scripted controller", "Controller selection", RepeatPolicy.IgnoreRepeat, controllerActive, new ModalCommand(/* ChooseSimulatorController */ 9, [MapController.Scripted]))), delay(() => append(singleton_1(ModalInput_binding("simulator.controller.general", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.SimulatorControllerSelection]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("g", ModalInput_plain, InputPhase.KeyDown), "Choose General AI controller", "Controller selection", RepeatPolicy.IgnoreRepeat, controllerActive, new ModalCommand(/* ChooseSimulatorController */ 9, [MapController.General]))), delay(() => append(singleton_1(ModalInput_binding("simulator.controller.commit", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.SimulatorControllerSelection]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Enter", ModalInput_plain, InputPhase.KeyDown), "Commit controller choice", "Controller selection", RepeatPolicy.IgnoreRepeat, controllerActive, ModalCommand.CommitSimulatorController)), delay(() => singleton_1(ModalInput_binding("simulator.controller.cancel", new ModalContextSelector(/* ExactContext */ 2, [ModalContext.SimulatorControllerSelection]), ModalPrecedence.ActiveGestureOrPreview, ModalInput_key("Escape", ModalInput_plain, InputPhase.KeyDown), "Cancel controller choice", "Controller selection", RepeatPolicy.IgnoreRepeat, controllerActive, ModalCommand.CancelSimulatorController)))))))))))));
}

export function ModalInput_traverseSimulatorUnit(delta, selectedUnitId, handoff) {
    const identifiers = sort(map_1((tuple) => (tuple[0] | 0), toArray(handoff.RuntimeMap.Units), Int32Array), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    if (identifiers.length === 0) {
        return undefined;
    }
    else {
        let currentIndex;
        const option_1 = selectedUnitId;
        if (option_1 != null) {
            const selected = option_1 | 0;
            currentIndex = tryFindIndex((y_1) => (selected === y_1), identifiers);
        }
        else {
            currentIndex = undefined;
        }
        return item((currentIndex == null) ? ((delta < 0) ? (identifiers.length - 1) : 0) : (((currentIndex + (delta % identifiers.length)) + identifiers.length) % identifiers.length), identifiers);
    }
}

function ModalInput_columnName(column) {
    const loop = (value_mut, suffix_mut) => {
        loop:
        while (true) {
            const value = value_mut, suffix = suffix_mut;
            const remainder = (value % 26) | 0;
            const letter = String.fromCharCode((~~"A".charCodeAt(0) + remainder) & 0xFFFF);
            const next = (~~(value / 26) - 1) | 0;
            if (next < 0) {
                return letter + suffix;
            }
            else {
                value_mut = next;
                suffix_mut = (letter + suffix);
                continue loop;
            }
            break;
        }
    };
    return loop(max_1(0, column), "");
}

function ModalInput_addressText(address) {
    return ModalInput_columnName(address.CellColumn) + int32ToString(address.CellRow + 1);
}

function ModalInput_terrainName(_arg) {
    switch (_arg.tag) {
        case 1:
            return "Rough";
        case 2:
            return "Blocked";
        case 3:
            return "Objective";
        default:
            return "Open";
    }
}

function ModalInput_terrainToolName(_arg) {
    switch (_arg.tag) {
        case 1:
            return "Rectangle";
        case 2:
            return "Line";
        case 3:
            return "Flood fill";
        case 4:
            return "Eyedropper";
        case 5:
            return "Erase";
        default:
            return "Pencil";
    }
}

function ModalInput_keyboardObjectName(_arg) {
    switch (_arg.tag) {
        case 1:
            return "region " + int32ToString(_arg.fields[0]);
        case 2:
            return toString(_arg.fields[2]) + " edge";
        case 3:
            return "terrain cell";
        default:
            return "unit " + int32ToString(_arg.fields[0]);
    }
}

export function ModalInput_projectEditor(facts, catalog) {
    let option_1, tupledArg, option_9, option_6, matchValue_3, option_12, option_15, option_18, tupledArg_1, kind_1, matchValue_6, preview, height_1, name, width_1, identifiers;
    const contexts = ModalInput_deriveEditorContexts(facts);
    let patternInput;
    const matchValue = facts.Editor.Gesture;
    const matchValue_1 = facts.Editor.Tool;
    switch (matchValue.tag) {
        case 2: {
            patternInput = [ofArray(["Editor", "Select", "Box selection"]), (("Anchor " + ModalInput_addressText(matchValue.fields[0])) + " — current ") + ModalInput_addressText(matchValue.fields[1])];
            break;
        }
        case 4: {
            patternInput = [ofArray(["Editor", "Units", "Move preview"]), (int32ToString(matchValue.fields[2].length) + " units — preview at ") + ModalInput_addressText(matchValue.fields[1])];
            break;
        }
        case 5: {
            const previewCount = defaultArg((option_1 = terrainPreview(facts.Editor), (option_1 != null) ? ((tupledArg = option_1, tupledArg[1].length)) : undefined), matchValue.fields[3].length) | 0;
            patternInput = [ofArray(["Editor", "Terrain", ModalInput_terrainToolName(matchValue.fields[0])]), ((((((ModalInput_terrainName(facts.Editor.TerrainSelection) + " terrain — anchor ") + ModalInput_addressText(matchValue.fields[1])) + ", endpoint ") + ModalInput_addressText(matchValue.fields[2])) + " — ") + int32ToString(previewCount)) + " cells"];
            break;
        }
        case 6: {
            patternInput = [ofArray(["Editor", "Edges", "Polyline"]), int32ToString(matchValue.fields[1].length) + " segments staged"];
            break;
        }
        case 3: {
            patternInput = ((matchValue.fields[0].tag === 2) ? ((matchValue_1.tag === 4) ? [ofArray(["Editor", "Units", "Place preview"]), (int32ToString(matchValue.fields[0].fields[0].length) + ((matchValue.fields[0].fields[0].length === 1) ? " unit footprint" : " unit footprints")) + " staged"] : [ofArray(["Editor", "Units", "Paste preview"]), (int32ToString(matchValue.fields[0].fields[0].length) + ((matchValue.fields[0].fields[0].length === 1) ? " unit" : " units")) + " staged — Enter commits one undoable command"]) : [ofArray(["Editor", "Command preview"]), "A validated editor command is ready to commit"]);
            break;
        }
        case 0: {
            switch (matchValue_1.tag) {
                case 2: {
                    patternInput = [ofArray(["Editor", "Terrain", ModalInput_terrainToolName(matchValue_1.fields[0])]), (((((ModalInput_terrainName(facts.Editor.TerrainSelection) + " terrain — ") + int32ToString(facts.Editor.BrushSize)) + "×") + int32ToString(facts.Editor.BrushSize)) + " brush — cursor ") + ModalInput_addressText(facts.Editor.TerrainCursor)];
                    break;
                }
                case 3: {
                    const visible = searchCanonicalUnitPresets(facts.Editor.UnitPaletteSearch);
                    const matchValue_4 = selectedUnitPalettePreset(facts.Editor);
                    if (matchValue_4 == null) {
                        patternInput = [ofArray(["Editor", "Units", "Browse"]), ("No presets match “" + facts.Editor.UnitPaletteSearch) + "”"];
                    }
                    else {
                        const preset = matchValue_4;
                        patternInput = [ofArray(["Editor", "Units", "Browse"]), (((((((((((preset.Faction + " / ") + preset.Name) + " — ") + int32ToString(preset.FootprintSize)) + "×") + int32ToString(preset.FootprintSize)) + " — ") + int32ToString(preset.HealthMaximum)) + " HP — preset ") + int32ToString(facts.Editor.UnitPaletteCursor.ResultIndex + 1)) + " of ") + int32ToString(length(visible))];
                    }
                    break;
                }
                case 4: {
                    patternInput = [ofArray(["Editor", "Units", "Place"]), (((((((((toString(matchValue_1.fields[0]) + " / ") + matchValue_1.fields[1]) + " — ") + int32ToString(matchValue_1.fields[2])) + "×") + int32ToString(matchValue_1.fields[2])) + " — preview at ") + ModalInput_addressText(facts.Editor.UnitPlacementCursor)) + " — ") + defaultArg((option_9 = unitPlacementIssue(facts.Editor), (option_9 != null) ? ("invalid: " + option_9) : undefined), "valid")];
                    break;
                }
                case 5: {
                    patternInput = [ofArray(["Editor", "Edges"]), (toString(matchValue_1.fields[1]) + " / ") + toString(matchValue_1.fields[0])];
                    break;
                }
                case 1: {
                    patternInput = [ofArray(["Editor", "Terrain", "Pencil"]), ModalInput_terrainName(matchValue_1.fields[0]) + " terrain"];
                    break;
                }
                default: {
                    const objects = keyboardObjectsAtCursor(facts.Editor);
                    let current_3;
                    const option_4 = facts.Editor.KeyboardObject;
                    current_3 = ((option_4 != null) ? option_4 : tryHead(objects));
                    patternInput = [ofArray(["Editor", "Select"]), (((("Cursor " + ModalInput_addressText(facts.Editor.KeyboardCursor.Cell)) + " — ") + int32ToString(FSharpSet__get_Count(facts.Editor.SelectedUnits))) + " units selected") + defaultArg((option_6 = current_3, (option_6 != null) ? ((((((" — " + ModalInput_keyboardObjectName(option_6)) + " (") + int32ToString(facts.Editor.KeyboardCursor.ObjectCycleIndex + 1)) + " of ") + int32ToString(length(objects))) + ")") : undefined), "")];
                }
            }
            break;
        }
        default:
            patternInput = [ofArray(["Editor", "Select", "Actions"]), (matchValue_3 = facts.Editor.SelectedRegion, (matchValue_3 == null) ? (int32ToString(FSharpSet__get_Count(facts.Editor.SelectedUnits)) + ((FSharpSet__get_Count(facts.Editor.SelectedUnits) === 1) ? " unit selected" : " units selected")) : (("Region " + int32ToString(matchValue_3)) + " selected"))];
    }
    let patternInput_2;
    const matchValue_5 = facts.Editor.RegionKeyboardMode;
    switch (matchValue_5.tag) {
        case 2: {
            patternInput_2 = [ofArray(["Editor", "Zones", "New", "Shape"]), toString(matchValue_5.fields[0]) + " — choose rectangle or polygon"];
            break;
        }
        case 3: {
            patternInput_2 = [ofArray(["Editor", "Zones", "New", "Rectangle"]), ("Cursor " + ModalInput_addressText(facts.Editor.KeyboardCursor.Cell)) + defaultArg((option_12 = matchValue_5.fields[1], (option_12 != null) ? (" — anchor " + ModalInput_addressText(option_12)) : undefined), " — choose first corner")];
            break;
        }
        case 4: {
            patternInput_2 = [ofArray(["Editor", "Zones", "New", "Polygon"]), (int32ToString(matchValue_5.fields[1].length) + " vertices — cursor ") + ModalInput_addressText(facts.Editor.KeyboardCursor.Cell)];
            break;
        }
        case 5: {
            patternInput_2 = [ofArray(["Editor", "Zones", "Move"]), facts.Editor.RegionAnnouncement];
            break;
        }
        case 6: {
            patternInput_2 = ((matchValue_5.fields[1].tag === 0) ? [ofArray(["Editor", "Zones", "Resize"]), ((int32ToString(matchValue_5.fields[1].fields[2]) + "×") + int32ToString(matchValue_5.fields[1].fields[3])) + " preview"] : [ofArray(["Editor", "Zones", "Resize"]), facts.Editor.RegionAnnouncement]);
            break;
        }
        case 7: {
            patternInput_2 = ((matchValue_5.fields[1].tag === 1) ? [ofArray(["Editor", "Zones", "Vertices"]), (("Vertex " + int32ToString(matchValue_5.fields[2] + 1)) + " of ") + int32ToString(matchValue_5.fields[1].fields[0].length)] : [ofArray(["Editor", "Zones", "Vertices"]), facts.Editor.RegionAnnouncement]);
            break;
        }
        case 0: {
            if (equals(facts.ActiveDomain, EditorDomain.RegionDomain)) {
                patternInput_2 = [ofArray(["Editor", "Zones"]), ("Cursor " + ModalInput_addressText(facts.Editor.KeyboardCursor.Cell)) + defaultArg((option_15 = facts.Editor.SelectedRegion, (option_15 != null) ? ((" — region " + int32ToString(option_15)) + " selected") : undefined), " — no region selected")];
            }
            else if (equals(facts.ActiveDomain, EditorDomain.DocumentDomain)) {
                patternInput_2 = [ofArray(["Editor", "Document"]), ((("Map “" + facts.Editor.Authoring.Name) + "” — revision ") + int64ToString(facts.Editor.Revision.Number)) + (equals(facts.Editor.RevisionState, RevisionState.DirtyRevision) ? " — dirty" : " — saved")];
            }
            else if (equals(facts.ActiveDomain, EditorDomain.EdgeDomain)) {
                const patternInput_1 = facts.Editor.EdgeCursor;
                const row = patternInput_1[1] | 0;
                const direction_1 = patternInput_1[2];
                const column = patternInput_1[0] | 0;
                const existing = defaultArg((option_18 = tryFind_1([column, row, direction_1], facts.Editor.Map.Edges), (option_18 != null) ? ((tupledArg_1 = option_18, (kind_1 = tupledArg_1[0], toString(kind_1) + (equals(kind_1, MapEdgeKind.Door) ? (tupledArg_1[1] ? " open" : " closed") : "")))) : undefined), "no edge");
                patternInput_2 = [ofArray(["Editor", "Edges"]), (((("Cursor edge " + ModalInput_addressText(new EditorCellAddress(column, row))) + " ") + toString(direction_1)) + " — ") + existing];
            }
            else {
                patternInput_2 = [patternInput[0], patternInput[1]];
            }
            break;
        }
        default:
            patternInput_2 = [toList(delay(() => append(singleton_1("Editor"), delay(() => append(singleton_1("Zones"), delay(() => (matchValue_5.fields[0] ? singleton_1("Purpose") : append(singleton_1("New"), delay(() => singleton_1("Purpose")))))))))), (matchValue_5.fields[0] ? "Change selected region purpose — " : "Choose region purpose — ") + toString(matchValue_5.fields[1])];
    }
    const underlyingDetail = patternInput_2[1];
    const underlyingBreadcrumb = patternInput_2[0];
    const patternInput_3 = (facts.Editor.PendingDestructiveChange != null) ? [ofArray(["Editor", "Destructive confirmation"]), (matchValue_6 = facts.Editor.PendingDestructiveChange, (matchValue_6 == null) ? underlyingDetail : ((matchValue_6.tag === 0) ? ((preview = matchValue_6.fields[0], (("Confirm resize to " + int32ToString(preview.TargetWidth)) + "×") + int32ToString(preview.TargetHeight))) : ((matchValue_6.tag === 2) ? ((height_1 = (matchValue_6.fields[1] | 0), (name = matchValue_6.fields[2], (width_1 = (matchValue_6.fields[0] | 0), (((("Confirm new map " + name) + " at ") + int32ToString(width_1)) + "×") + int32ToString(height_1))))) : ((matchValue_6.tag === 3) ? ((identifiers = matchValue_6.fields[0], ("Confirm deleting " + int32ToString(identifiers.length)) + ((identifiers.length === 1) ? " unit" : " units"))) : "Confirm clearing the current map"))))] : (facts.PanHeld ? [ofArray(["Editor", "Pan held"]), "Underlying mode: " + join(" / ", tail(underlyingBreadcrumb))] : [underlyingBreadcrumb, underlyingDetail]);
    const breadcrumb = patternInput_3[0];
    return new ModalProjection$1(contexts, breadcrumb, join(" / ", map((_arg_2) => _arg_2.toUpperCase(), breadcrumb)), patternInput_3[1], ModalInput_possibleInputs(contexts, catalog));
}

export function ModalInput_projectSimulator(facts, selectedUnitId, handoff, catalog) {
    let option_1, simulator_1, preview, option_4;
    const contexts = ModalInput_deriveSimulatorContexts(facts);
    let patternInput;
    if (handoff != null) {
        if (facts.SimulatorControllerSelection != null) {
            const simulator_2 = handoff;
            const choice = value_26(facts.SimulatorControllerSelection);
            patternInput = [ofArray(["Simulator", "Controller"]), (("Unit " + defaultArg((option_1 = selectedUnitId, (option_1 != null) ? int32ToString(option_1) : undefined), "not selected")) + " — ") + controllerLabel(choice)];
        }
        else if ((simulator_1 = handoff, (simulator_1.PreviewDestination != null) && !simulator_1.IsRunning)) {
            const simulator_3 = handoff;
            const destination = value_26(simulator_3.PreviewDestination);
            const route = MapEditorSimulator_preview(selectedUnitId, destination, simulator_3);
            patternInput = [ofArray(["Simulator", "Route preview"]), (route == null) ? (("Route preview at " + ModalInput_addressText(destination)) + " — select a unit") : ((preview = route, ((((((("Unit " + int32ToString(preview.UnitId)) + " → ") + ModalInput_addressText(destination)) + " — ") + (equals(preview.Collision, SimulatorCollision.RouteClear) ? "route clear" : toString(preview.Collision))) + " — ") + int32ToString(preview.DistanceMillimeters)) + " mm"))];
        }
        else {
            const simulator_4 = handoff;
            patternInput = [toList(delay(() => append(singleton_1("Simulator"), delay(() => (simulator_4.IsRunning ? singleton_1("Running") : singleton_1("Paused")))))), (((("Revision " + int64ToString(simulator_4.Revision.Number)) + " — tick ") + int32ToString(simulator_4.Tick)) + defaultArg((option_4 = selectedUnitId, (option_4 != null) ? ((" — unit " + int32ToString(option_4)) + " selected") : undefined), "")) + (facts.SimulatorRevisionIsStale ? " — revision stale" : "")];
        }
    }
    else {
        patternInput = [ofArray(["Simulator", "Unavailable"]), "Correct the current map so a simulation can be maintained"];
    }
    const breadcrumb = patternInput[0];
    return new ModalProjection$1(contexts, breadcrumb, join(" / ", map((_arg) => _arg.toUpperCase(), breadcrumb)), patternInput[1], ModalInput_possibleInputs(contexts, catalog));
}

export function ModalInput_validateCatalog(catalog) {
    return sortBy((_arg_1) => {
        if (_arg_1.tag === 1) {
            return [1, _arg_1.fields[0], _arg_1.fields[1]];
        }
        else {
            return [0, _arg_1.fields[0], ""];
        }
    }, append_1(choose((tupledArg) => {
        if (length(tupledArg[1]) > 1) {
            return new CatalogDiagnostic(/* DuplicateBindingId */ 0, [tupledArg[0]]);
        }
        else {
            return undefined;
        }
    }, List_groupBy((binding) => binding.Id, catalog, {
        Equals: (x, y) => (x === y),
        GetHashCode: (x) => (stringHash(x) | 0),
    })), sortBy((_arg) => {
        if (_arg.tag === 0) {
            return [_arg.fields[0], ""];
        }
        else {
            return [_arg.fields[0], _arg.fields[1]];
        }
    }, List_distinct(collect_1((tupledArg_1) => {
        const first = tupledArg_1[1];
        return choose((second) => {
            if ((equals(first.Precedence, second.Precedence) && ModalInput_sameGesture(first.BindingGesture, second.BindingGesture)) && ModalInput_selectorsOverlap(first.Context, second.Context)) {
                const patternInput = (first.Id <= second.Id) ? [first.Id, second.Id] : [second.Id, first.Id];
                return new CatalogDiagnostic(/* EqualPrecedenceGestureConflict */ 1, [patternInput[0], patternInput[1], first.Precedence, first.BindingGesture]);
            }
            else {
                return undefined;
            }
        }, skip(tupledArg_1[0] + 1, catalog));
    }, indexed(catalog)), {
        Equals: equals,
        GetHashCode: (x_1) => (safeHash(x_1) | 0),
    }), {
        Compare: (x_2, y_2) => (compareArrays(x_2, y_2) | 0),
    })), {
        Compare: (x_3, y_3) => (compareArrays(x_3, y_3) | 0),
    });
}

